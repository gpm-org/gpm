using System;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Tasks
{
    public static class Update
    {
        public static async Task Action(string name, bool all, int slot, bool clean, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var loggerService = serviceProvider.GetRequiredService<ILoggerService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();

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
                }
                else
                {
                    // ignore all
                    await UpdatePackage(name, clean, slot);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(name))
                {
                    loggerService.Warning($"No package name specified. To update all installed packages use gpm update --all.");
                    return;
                }
                // update package in slot
                await UpdatePackage(name, clean, slot);
            }

            async Task UpdatePackage(string nameInner, bool cleanInner, int slotIdx = 0)
            {
                // checks
                var package = dataBaseService.GetPackageFromName(nameInner);
                if (package is null)
                {
                    loggerService.Warning($"Package {nameInner} not found in database.");
                    return;
                }
                if (!libraryService.TryGetValue(package.Id, out var model))
                {
                    loggerService.Warning($"[{package.Id}] Package not found in library. Use gpm install to install a package.");
                    return;
                }
                if (!libraryService.IsInstalled(package))
                {
                    loggerService.Warning($"[{package.Id}] Package not installed. Use gpm install to install a package.");
                    return;
                }
                if (!libraryService.IsInstalledInSlot(package, slotIdx))
                {
                    loggerService.Warning($"[{package.Id}] Package not installed in slot {slotIdx.ToString()}. Use gpm install to install a package.");
                    return;
                }

                var slotInner = model.Slots[slotIdx];
                var releases = await gitHubService.GetReleasesForPackage(package);
                if (releases is null || !releases.Any())
                {
                    loggerService.Warning($"No releases found for package {package.Id}");
                    return;
                }

                ArgumentNullException.ThrowIfNull(slotInner.Version);
                if (!gitHubService.IsUpdateAvailable(package, releases, slotInner.Version))
                {
                    return;
                }

                if (cleanInner)
                {
                    loggerService.Information($"[{package}] Removing installed package ...");
                    if (libraryService.UninstallPackage(package, slotIdx))
                    {
                        loggerService.Success($"[{package}] Old package successfully removed.");
                    }
                    else
                    {
                        loggerService.Warning($"[{package}] Failed to remove installed package. Aborting.");
                        return;
                    }
                }

                loggerService.Information($"[{package}] Updating package ...");

                if (await gitHubService.InstallReleaseAsync(package, releases, null, slotIdx))
                {
                    loggerService.Success($"[{package}] Package successfully updated to version {model.Slots[slotIdx].Version}.");
                }
                else
                {
                    loggerService.Warning($"[{package}] Failed to update package.");
                }
            }
        }
    }
}
