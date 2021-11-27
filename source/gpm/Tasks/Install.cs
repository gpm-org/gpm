using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Extensions;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gpm.Tasks
{
    public static class Install
    {
        public static async Task Action(string name, string version, string slot, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(Install));
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            if (string.IsNullOrEmpty(name))
            {
                logger.LogWarning($"No package name specified to install.");
                return;
            }
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                logger.LogWarning("[{Package}] Package {Name} not found", package, name);
                return;
            }

            var slotId = 0;
            if (!string.IsNullOrEmpty(slot))
            {
                if (!Directory.Exists(slot))
                {
                    logger.LogWarning("[{Package}] No valid directory path given for slot {Slot}", package, slot);
                    return;
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
                    logger.LogWarning("[{Package}] Already installed in slot {Slot} - Use gpm update or gpm repair",
                        package, slot);
                    return;
                }
            }

            logger.LogInformation("[{Package}] Installing package ...", package);
            var releases = await gitHubService.GetReleasesForPackage(package);
            if (releases is null || !releases.Any())
            {
                logger.LogWarning("No releases found for package {Package}", package);
                return;
            }

            if (await gitHubService.InstallReleaseAsync(package, releases, version, slotId))
            {
                logger.LogDebug("[{Package}] Package successfully installed", package);
            }
        }
    }
}
