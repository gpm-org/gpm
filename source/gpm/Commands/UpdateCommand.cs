using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class UpdateCommand : Command
    {
        private IDataBaseService? _dataBaseService;
        private ILoggerService? _logger;
        private ILibraryService? _libraryService;
        private IGitHubService? _gitHubService;

        private new const string Description = "Update an installed package.";
        private new const string Name = "update";

        public UpdateCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));

            AddOption(new Option<bool>(new[] { "--all", "-a" },
                "Update all installed packages in the default slot."));
            AddOption(new Option<int>(new[] { "--slot", "-s" },
                "Update a specific slot. Default is 0."));

            Handler = CommandHandler.Create<string, bool, int, IHost>(Action);
        }

        private async Task Action(string name, bool all, int slot, IHost host)
        {
            var serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();
            _logger = serviceProvider.GetRequiredService<ILoggerService>();
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
                        await UpdatePackage(key);
                    }
                }
                else
                {
                    // ignore all
                    await UpdatePackage(name, slot);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(name))
                {
                    _logger.Error($"No package name specified. To update all installed packages use gpm update --all.");
                    return;
                }
                // update package in slot
                await UpdatePackage(name, slot);
            }
        }



        private async Task UpdatePackage(string key, int slot = 0)
        {
            ArgumentNullException.ThrowIfNull(_dataBaseService);
            ArgumentNullException.ThrowIfNull(_logger);
            ArgumentNullException.ThrowIfNull(_libraryService);
            ArgumentNullException.ThrowIfNull(_gitHubService);

            // checks
            var package = _dataBaseService.GetPackageFromName(key);
            if (package is null)
            {
                _logger.Error($"Package {key} not found in database.");
                return;
            }
            if (!_libraryService.TryGetValue(key, out var model))
            {
                _logger.Error($"[{package.Id}] Package not found in library. Use gpm install to install a package.");
                return;
            }
            if (!_libraryService.IsInstalled(package))
            {
                _logger.Error($"[{package.Id}] Package not installed. Use gpm install to install a package.");
                return;
            }
            if (!_libraryService.IsInstalledInSlot(package, slot))
            {
                _logger.Error($"[{package.Id}] Package not installed in slot {slot.ToString()}. Use gpm install to install a package.");
                return;
            }

            _logger.Info($"[{package}] Updating package ...");

            if (await _gitHubService.InstallReleaseAsync(package, null, slot))
            {
                _logger.Success($"[{package}] Package successfully updated to version {model.Slots[slot].Version}.");
            }
            else
            {
                _logger.Error($"[{package}] Failed to update package ...");
            }
        }
    }
}
