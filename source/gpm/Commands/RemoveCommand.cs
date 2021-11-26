using System;
using System.Collections.Generic;
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

            if (string.IsNullOrEmpty(name))
            {
                _logger.Warning($"No package name specified to install.");
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
                    _libraryService.UninstallPackage(package, slotIdx);
                }
            }
            else
            {
                _libraryService.UninstallPackage(package, slot);
            }
        }
    }
}
