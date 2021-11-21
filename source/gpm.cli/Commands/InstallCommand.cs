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
    public class InstallCommand : Command
    {
        #region Fields

        private new const string Description = "";
        private new const string Name = "install";

        private IServiceProvider? _serviceProvider;
        private ILoggerService? _logger;
        private IDataBaseService? _dataBaseService;

        #endregion Fields

        public InstallCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string>("name",
                "The package name. Can be a github repo url, a repo name or in the form of owner/name/id"));

            AddOption(new Option<string>(new[] { "--version", "-v" },
                "The package version to install."));

            Handler = CommandHandler.Create<string, string, IHost>(Action);
        }

        private async Task Action(string name, string version, IHost host)
        {
            _serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(_serviceProvider);

            _logger = _serviceProvider.GetRequiredService<ILoggerService>();
            _dataBaseService = _serviceProvider.GetRequiredService<IDataBaseService>();
            var githubService = _serviceProvider.GetRequiredService<IGitHubService>();
            var libraryService = _serviceProvider.GetRequiredService<ILibraryService>();

            ArgumentNullException.ThrowIfNull(_logger);
            ArgumentNullException.ThrowIfNull(_dataBaseService);
            ArgumentNullException.ThrowIfNull(githubService);
            ArgumentNullException.ThrowIfNull(libraryService);

            var package = GetPackageFromName(name);
            if (package is null)
            {
                _logger.Error($"package {name} not found");
                return;
            }

            var existingModel = libraryService.Lookup(package.Id);
            if (existingModel.HasValue)
            {
                if (existingModel.Value.Manifests.ContainsKey(version))
                {
                    _logger.Warning($"package {name} with version {version} already installed. To reinstall use gpm repair.");
                    // TODO: ask to reinstall?
                    return;
                }
            }

            _logger.Info($"Installing package {package.Id}...");

            await githubService.InstallReleaseAsync(package, version);

            _logger.Success($"Package {package.Id} successfully installed.");
        }

        private Package? GetPackageFromName(string name)
        {
            ArgumentNullException.ThrowIfNull(_logger);
            ArgumentNullException.ThrowIfNull(_dataBaseService);

            if (Path.GetExtension(name) == ".git")
            {
                var packages = _dataBaseService.LookupByUrl(name).ToList();
                if (packages.Count == 1)
                {
                    return packages.First();
                }
                else if (packages.Count > 1)
                {
                    // log results
                    foreach (var item in packages)
                    {
                        _logger.Warning($"Multiple packages found in repository {name}:");
                        _logger.Info(item.Id);
                    }
                }
            }
            else if (name.Split('/').Length == 2)
            {
                var splits = name.Split('/');
                var id = $"{splits[0]}-{splits[1]}";
                return _dataBaseService.Lookup(id);
            }
            else if (name.Split('/').Length == 3)
            {
                var splits = name.Split('/');
                var id = $"{splits[0]}-{splits[1]}-{splits[2]}";
                return _dataBaseService.Lookup(id);
            }
            else
            {

                {
                    // try name
                    var packages = _dataBaseService.LookupByName(name).ToList();
                    if (packages.Count == 1)
                    {
                        return packages.First();
                    }
                    else if (packages.Count > 1)
                    {
                        // log results
                        foreach (var item in packages)
                        {
                            _logger.Warning($"Multiple packages found for name {name}:");
                            _logger.Info(item.Id);
                        }
                    }
                }

                {
                    // try owner
                    var packages = _dataBaseService.LookupByOwner(name).ToList();
                    if (packages.Count == 1)
                    {
                        return packages.First();
                    }
                    else if (packages.Count > 1)
                    {
                        // log results
                        foreach (var item in packages)
                        {
                            _logger.Warning($"Multiple packages found for owner {name}:");
                            _logger.Info(item.Id);
                        }
                    }
                }
            }

            return null;
        }

    }
}
