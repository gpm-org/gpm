using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class UpdateCommand : Command
    {
        private IDataBaseService? _dataBaseService;
        private ILoggerService? _loggerService;
        private ILibraryService? _libraryService;
        private IGitHubService? _gitHubService;

        private new const string Description = "Update an installed package.";
        private new const string Name = "update";

        public UpdateCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));

            AddOption(new Option<bool>(new[] { "--all", "-a" },
                "Update all installed packages in their default slot"));
            AddOption(new Option<int>(new[] { "--slot", "-s" },
                "Update a specific slot. Input the index of the slot, default is 0."));

            AddOption(new Option<bool>(new[] { "--clean", "-c" },
                "Do a clean install and completely remove the installed package."));

            Handler = CommandHandler.Create<string, bool, int, bool, IHost>(Action);
        }

        private async Task Action(string name, bool all, int slot, bool clean, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            _loggerService = serviceProvider.GetRequiredService<ILoggerService>();
            _libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            _gitHubService = serviceProvider.GetRequiredService<IGitHubService>();

            // checks
            if (all)
            {
                if (string.IsNullOrEmpty(name))
                {
                    // add all installed packages and use default slot
                    foreach (var (key, _) in _libraryService)
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
                    _loggerService.Error($"No package name specified. To update all installed packages use gpm update --all.");
                    return;
                }
                // update package in slot
                await UpdatePackage(name, clean, slot);
            }
        }



        private async Task UpdatePackage(string name, bool clean, int slotIdx = 0)
        {
            ArgumentNullException.ThrowIfNull(_dataBaseService);
            ArgumentNullException.ThrowIfNull(_loggerService);
            ArgumentNullException.ThrowIfNull(_libraryService);
            ArgumentNullException.ThrowIfNull(_gitHubService);

            // checks
            var package = _dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                _loggerService.Warning($"Package {name} not found in database.");
                return;
            }
            if (!_libraryService.TryGetValue(package.Id, out var model))
            {
                _loggerService.Warning($"[{package.Id}] Package not found in library. Use gpm install to install a package.");
                return;
            }
            if (!_libraryService.IsInstalled(package))
            {
                _loggerService.Warning($"[{package.Id}] Package not installed. Use gpm install to install a package.");
                return;
            }
            if (!_libraryService.IsInstalledInSlot(package, slotIdx))
            {
                _loggerService.Warning($"[{package.Id}] Package not installed in slot {slotIdx.ToString()}. Use gpm install to install a package.");
                return;
            }

            var slot = model.Slots[slotIdx];
            var releases = await _gitHubService.GetReleasesForPackage(package);
            if (releases is null || !releases.Any())
            {
                _loggerService.Warning($"No releases found for package {package.Id}");
                return;
            }

            ArgumentNullException.ThrowIfNull(slot.Version);
            if (!_gitHubService.IsUpdateAvailable(package, releases, slot.Version))
            {
                return;
            }

            if (clean)
            {
                _loggerService.Info($"[{package}] Removing installed package ...");
                if (_libraryService.UninstallPackage(package, slotIdx))
                {
                    _loggerService.Success($"[{package}] Old package successfully removed.");
                }
                else
                {
                    _loggerService.Error($"[{package}] Failed to remove installed package. Aborting.");
                    return;
                }
            }

            _loggerService.Info($"[{package}] Updating package ...");

            if (await _gitHubService.InstallReleaseAsync(package, releases, null, slotIdx))
            {
                _loggerService.Success($"[{package}] Package successfully updated to version {model.Slots[slotIdx].Version}.");
            }
            else
            {
                _loggerService.Error($"[{package}] Failed to update package.");
            }
        }
    }
}
