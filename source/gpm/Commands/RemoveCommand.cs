using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class RemoveCommand : Command
    {
        private new const string Description = "";
        private new const string Name = "remove";

        private ILibraryService? _libraryService;
        private ILoggerService? _logger;

        public RemoveCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));

            // TODO: slots and removing all packages
            AddOption(new Option<int>(new[] { "--slot", "-s" },
                "The package slot to remove."));
            AddOption(new Option<bool>(new[] { "--all", "-a" },
                "Remove package from all installed slots."));

            Handler = CommandHandler.Create<string, int, bool, IHost>(Action);
        }

        private void Action(string name, int slot, bool all, IHost host)
        {
            var serviceProvider = host.Services;
            _logger = serviceProvider.GetRequiredService<ILoggerService>();
            ArgumentNullException.ThrowIfNull(_logger);
            _libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            ArgumentNullException.ThrowIfNull(_libraryService);
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            // TODO support gpm remove --all?
            if (string.IsNullOrEmpty(name))
            {
                _logger.Error($"No package name specified to install.");
                return;
            }

            // what if a package is removed from the database?
            // deprecate instead?
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                _logger.Warning($"package {name} not found in database");
                return;
            }
            if (!_libraryService.TryGetValue(package.Id, out var model))
            {
                _logger.Warning($"[{package.Id}] Package is not installed.");
                return;
            }
            if (!_libraryService.IsInstalled(package))
            {
                _logger.Warning($"[{package.Id}] Package is not installed.");
                return;
            }

            // check if all is set
            if (all)
            {
                foreach (var (slotIdx, _) in model.Slots)
                {
                    RemovePackage(package, model, slotIdx);
                }
            }
            else
            {
                RemovePackage(package, model, slot);
            }
        }

        private void RemovePackage(Package package, PackageModel model, int slotIdx)
        {
            ArgumentNullException.ThrowIfNull(_logger);
            ArgumentNullException.ThrowIfNull(_libraryService);

            if (!model.Slots.TryGetValue(slotIdx, out var slot))
            {
                _logger.Warning($"[{package.Id}] No package installed in slot {slotIdx.ToString()}.");
                return;
            }

            _logger.Info($"[{package.Id}] Removing package from slot {slotIdx.ToString()}.");

            foreach (var file in slot.Files.Select(x => x.Name))
            {
                if (!File.Exists(file))
                {
                    _logger.Warning($"[{package.Id}] Could not find file {file} to delete. Skipping.");
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    _logger.Error($"[{package.Id}] Could not delete file {file}. Skipping.");
                    _logger.Error(e);
                }
            }

            // remove deploy manifest from library
            slot.Files.Clear();

            // TODO: remove cached files as well?
            _libraryService.Save();

            _logger.Success($"[{package.Id}] Successfully removed package.");
        }
    }
}
