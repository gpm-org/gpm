using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Extensions;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class InstallCommand : Command
    {
        private new const string Description = "Install a package.";
        private new const string Name = "install";

        public InstallCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id. "));

            AddOption(new Option<string>(new[] { "--version", "-v" },
                "A specific package version to install. Leave out or leave empty to install latest."));
            AddOption(new Option<string>(new[] { "--slot", "-s" },
                "A slot to install a new instance into. Must be a directory path."));

            Handler = CommandHandler.Create<string, string, string, IHost>(Action);
        }

        private async Task Action(string name, string version, string slot, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            if (string.IsNullOrEmpty(name))
            {
                logger.Error($"No package name specified to install.");
                return;
            }
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                logger.Error($"[{package}] Package {name} not found.");
                return;
            }

            var slotId = 0;
            if (!string.IsNullOrEmpty(slot))
            {
                if (!Directory.Exists(slot))
                {
                    logger.Error($"[{package}] No valid directory path given for slot {slot}.");
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

            logger.Info($"[{package}] Installing package ...");
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
