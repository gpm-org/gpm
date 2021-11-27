using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Extensions;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Tasks
{
    public static class Install
    {
        public static async Task Action(string name, string version, string slot, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            if (string.IsNullOrEmpty(name))
            {
                logger.Warning($"No package name specified to install.");
                return;
            }
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                logger.Warning($"[{package}] Package {name} not found.");
                return;
            }

            var slotId = 0;
            if (!string.IsNullOrEmpty(slot))
            {
                if (!Directory.Exists(slot))
                {
                    logger.Warning($"[{package}] No valid directory path given for slot {slot}.");
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
                    logger.Warning($"[{package}] Already installed in slot {slot} - Use gpm update or gpm repair.");
                    return;
                }
            }

            logger.Information($"[{package}] Installing package ...");
            var releases = await gitHubService.GetReleasesForPackage(package);
            if (releases is null || !releases.Any())
            {
                logger.Warning($"No releases found for package {package.Id}");
                return;
            }

            if (await gitHubService.InstallReleaseAsync(package, releases, version, slotId))
            {
                logger.Success($"[{package}] Package successfully installed.");
            }
        }
    }
}
