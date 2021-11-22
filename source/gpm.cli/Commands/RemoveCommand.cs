using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Models;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.cli.Commands
{
    public class RemoveCommand : Command
    {
        private new const string Description = "";
        private new const string Name = "remove";
        private ILoggerService? _logger;

        public RemoveCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));

            AddOption(new Option<string>(new[] { "--version", "-v" },
                "The package version to remove."));
            AddOption(new Option<bool>(new[] { "--all", "-a" },
                "Remove all installed versions of this package."));

            Handler = CommandHandler.Create<string, string, bool, IHost>(Action);
        }

        private void Action(string name, string version, bool all, IHost host)
        {
            var _serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(_serviceProvider);
            _logger = _serviceProvider.GetRequiredService<ILoggerService>();
            ArgumentNullException.ThrowIfNull(_logger);

            var _dataBaseService = _serviceProvider.GetRequiredService<IDataBaseService>();
            var libraryService = _serviceProvider.GetRequiredService<ILibraryService>();

            // TODO: check the git database?
            // what if a package is removed from the database?
            // don't do it? deprecate instead?
            var package = _dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                _logger.Error($"package {name} not found");
                return;
            }

            // no package installed with that id
            var optional = libraryService.Lookup(package.Id);
            if (!optional.HasValue)
            {
                _logger.Warning($"package {name} with version {version} is not installed.");
                return;
            }
            // package is in library but has not been downloaded or deployed
            var model = optional.Value;
            if (!model.Manifests.Any())
            {
                // remove from library
                libraryService.Remove(package.Id);

                _logger.Warning($"package {name} with version {version} is not installed.");
                return;
            }

            // if version is null or empty
            if (string.IsNullOrEmpty(version))
            {
                if (all)
                {
                    foreach (var (ver, manifest) in model.Manifests)
                    {
                        RemovePackage(package, model, ver);
                    }
                }
                else
                {
                    _logger.Warning($"No package version selected to remove. To remove all installed versions use gpm remove <PACKAGE> --all");
                    return;
                }
            }
            else
            {
                RemovePackage(package, model, version);
            }

            // TODO: remove all?
            
        }

        private void RemovePackage(Package package, PackageModel existingModel, string version)
        {
            ArgumentNullException.ThrowIfNull(_logger);

            if (!existingModel.Manifests.ContainsKey(version))
            {
                _logger.Warning($"package {package.Id} with version {version} is not installed.");
                return;
            }

            var deploymanifest = existingModel.Manifests[version].DeployManifest;
            if (deploymanifest is null)
            {
                _logger.Warning($"package {package.Id} with version {version} is not installed.");
                return;
            }

            if (deploymanifest.Files is null)
            {
                _logger.Warning($"package {package.Id} with version {version} is not installed.");
                return;
            }

            _logger.Info($"Removing package {package.Id}...");

            foreach (var file in deploymanifest.Files.Select(x => x.Name))
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Could not delete file {file}.");
                        _logger.Error(e);
                    }
                }
            }

            _logger.Success($"Package {package.Id} successfully removed.");
        }
    }
}
