using System;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public static class Update
    {
        public static async Task<bool> Action(string name, bool all, int slot, bool clean, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();
            var deploymentService = serviceProvider.GetRequiredService<IDeploymentService>();

            // update here
            Upgrade.Action(host);

            // checks
            if (all)
            {
                if (string.IsNullOrEmpty(name))
                {
                    // add all installed packages and use default slot
                    foreach (var (key, _) in libraryService)
                    {
                        await UpdatePackage(key, clean);
                    }

                    return true;
                }

                // ignore all
                return await UpdatePackage(name, clean, slot);
            }

            if (!string.IsNullOrEmpty(name))
            {
                return await UpdatePackage(name, clean, slot);
            }

            Log.Warning("No package name specified. To update all installed packages use gpm update --all");
            return false;
            // update package in slot

            async Task<bool> UpdatePackage(string nameInner, bool cleanInner, int slotIdx = 0)
            {
                // checks
                var package = dataBaseService.GetPackageFromName(nameInner);
                if (package is null)
                {
                    Log.Warning("Package {NameInner} not found in database", nameInner);
                    return false;
                }
                if (!libraryService.TryGetValue(package.Id, out var model))
                {
                    Log.Warning("[{Package}] Package not found in library. Use gpm install to install a package", package);
                    return false;
                }
                if (!libraryService.IsInstalled(package))
                {
                    Log.Warning("[{Package}] Package not installed. Use gpm install to install a package", package);
                    return false;
                }
                if (!libraryService.IsInstalledInSlot(package, slotIdx))
                {
                    Log.Warning(
                        "[{Package}] Package not installed in slot {SlotIdx}. Use gpm install to install a package",
                        package, slotIdx.ToString());
                    return false;
                }

                var slotInner = model.Slots[slotIdx];
                var releases = await gitHubService.GetReleasesForPackage(package);
                if (releases is null || !releases.Any())
                {
                    Log.Warning("[{Package}] No releases found for package", package);
                    return false;
                }

                ArgumentNullException.ThrowIfNull(slotInner.Version);
                if (!gitHubService.IsUpdateAvailable(package, releases, slotInner.Version))
                {
                    return false;
                }

                if (cleanInner)
                {
                    Log.Information("[{Package}] Removing installed package ...", package);
                    if (libraryService.UninstallPackage(package, slotIdx))
                    {
                        Log.Information("[{Package}] Old package successfully removed", package);
                    }
                    else
                    {
                        Log.Warning("[{Package}] Failed to remove installed package. Aborting", package);
                        return false;
                    }
                }

                Log.Information("[{Package}] Updating package ...", package);

                if (await deploymentService.InstallReleaseAsync(package, releases, null, slotIdx))
                {
                    Log.Information("[{Package}] Package successfully updated to version {Version}", package,
                        model.Slots[slotIdx].Version);
                    return true;
                }
                else
                {
                    Log.Warning("[{Package}] Failed to update package", package);
                    return false;
                }
            }
        }
    }
}
