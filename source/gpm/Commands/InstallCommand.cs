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
        private new const string Description = "";
        private new const string Name = "install";

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
            var _serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(_serviceProvider);

            var _logger = _serviceProvider.GetRequiredService<ILoggerService>();
            var _dataBaseService = _serviceProvider.GetRequiredService<IDataBaseService>();
            var githubService = _serviceProvider.GetRequiredService<IGitHubService>();
            var libraryService = _serviceProvider.GetRequiredService<ILibraryService>();

            var package = _dataBaseService.GetPackageFromName(name);
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
    }
}
