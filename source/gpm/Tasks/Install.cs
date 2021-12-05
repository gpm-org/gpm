using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Extensions;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public static class Install
    {
        public static async Task<bool> Action(string name, string version, string slot, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var deploymentService = serviceProvider.GetRequiredService<IDeploymentService>();

            if (Ensure.IsNotNullOrEmpty(name, () => Log.Warning($"No package name specified to install.")))
            {
                return false;
            }

            // update here
            Upgrade.Action(host);

            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                Log.Warning("[{Package}] Package {Name} not found", package, name);
                return false;
            }

            var slotId = 0;
            if (!string.IsNullOrEmpty(slot))
            {
                if (!Directory.Exists(slot))
                {
                    Log.Warning("[{Package}] No valid directory path given for slot {Slot}", package, slot);
                    return false;
                }

                //TODO: do I wanna allow that?
                // check if that slot is used by another package
                var isUsedByOtherPackage = libraryService.Values
                    .SelectMany(x => x.Slots.Values)
                    .Any(x => x.FullPath != null && x.FullPath.Equals(slot));
                if (isUsedByOtherPackage)
                {
                    Log.Warning("[{Package}] Already installed in slot {Slot} - Use gpm update or gpm repair",
                        package, slot);
                    return true;
                }

                // check if package is in local library
                // if not it just goes to slot 0
                var model = libraryService.GetOrAdd(package);

                // check if that path matches any slot
                // if not, add to a new slot
                // if it is, return because we should use update or repair
                var slotForPath = model.Slots.Values
                    .FirstOrDefault(x => x.FullPath != null && x.FullPath.Equals(slot));
                if (slotForPath is null)
                {
                    slotId = model.Slots.Count;
                    var slotManifest = model.Slots.GetOrAdd(slotId);
                    slotManifest.FullPath = slot;
                }
                else
                {
                    Log.Warning("[{Package}] Already installed in slot {Slot} - Use gpm update or gpm repair",
                        package, slot);
                    return false;
                }
            }

            Log.Information("[{Package}] Installing package ...", package);
            var releases = await gitHubService.GetReleasesForPackage(package);
            if (releases is null || !releases.Any())
            {
                Log.Warning("No releases found for package {Package}", package);
                return false;
            }

            if (await deploymentService.InstallReleaseAsync(package, releases, version, slotId))
            {
                Log.Information("[{Package}] Package successfully installed", package);
            }
            else
            {
                return false;
            }

            // dependencies
            var dependencies = package.Dependencies;
            if (dependencies is null)
            {
                return true;
            }

            var dependencyResult = true;
            if (dependencies.Length > 0)
            {
                // TODO install dependencies: versions, paths?
                Log.Information("[{Package}] Found {Count} dependencies. Installing...", package, dependencies.Length);
                foreach (var dep in dependencies)
                {
                    dependencyResult = await Action(dep, "", "", host);
                }
            }

            if (!dependencyResult)
            {
                Log.Warning("[{Package}] Some dependencies failed to install correctly", package);
            }
            else
            {
                Log.Information("[{Package}] All dependencies installed successfully", package);
            }

            return true;
        }
    }
}
