using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using gpm.core.Models;
using gpm.core.Services;
using gpm.core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octokit;

namespace gpm.cli.Commands
{
    public class InstallCommand : Command
    {
        #region Fields

        private new const string Description = "";
        private new const string Name = "install";

        private IServiceProvider? serviceProvider;
        private ILoggerService? logger;
        private IDataBaseService? db;

        #endregion Fields

        public InstallCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string[]>("names", "A package name or list of names. Can be a github repo url, a repo name or in the form of owner/name/id"));

            Handler = CommandHandler.Create<string[], IHost>(Action);
        }

        private void Action(string[] names, IHost host)
        {
            serviceProvider = host.Services;
            ArgumentNullException.ThrowIfNull(serviceProvider);

            logger = serviceProvider.GetRequiredService<ILoggerService>();
            db = serviceProvider.GetRequiredService<IDataBaseService>();

            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(db);

            if (names == null)
            {
                logger.Error("Please input a package name");
                return;
            }

            var toInstall = new List<Package>();
            foreach (var name in names.Select(x => x.ToLower()))
            {
                var package = Parsepackagename(name);

                if (package is not null)
                {
                    if (!toInstall.Contains(package))
                    {
                        toInstall.Add(package);
                    }
                }
                else
                {
                    logger.Error($"package {name} not found");
                }
            }

            //var tasks = new List<Task>();
            foreach (var package in toInstall.Distinct())
            {
                //tasks.Add(InstallPackageAsync(package));
                _ = InstallPackageAsync(package);
            }

            //await Task.WhenAll(tasks);
        }

        private Package? Parsepackagename(string name)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(db);

            if (Path.GetExtension(name) == ".git")
            {
                var packages = db.LookupByUrl(name).ToList();
                if (packages.Count == 1)
                {
                    return packages.First();
                }
                else if (packages.Count > 1)
                {
                    // log results
                    foreach (var item in packages)
                    {
                        logger.Warning($"Multiple packages found in repository {name}:");
                        logger.Info(item.Id);
                    }
                }
            }
            else if (name.Split('/').Length == 2)
            {
                var splits = name.Split('/');
                var id = $"{splits[0]}-{splits[1]}";
                return db.Lookup(id);
            }
            else if (name.Split('/').Length == 3)
            {
                var splits = name.Split('/');
                var id = $"{splits[0]}-{splits[1]}-{splits[2]}";
                return db.Lookup(id);
            }
            else
            {

                {
                    // try name
                    var packages = db.LookupByName(name).ToList();
                    if (packages.Count == 1)
                    {
                        return packages.First();
                    }
                    else if (packages.Count > 1)
                    {
                        // log results
                        foreach (var item in packages)
                        {
                            logger.Warning($"Multiple packages found for name {name}:");
                            logger.Info(item.Id);
                        }
                    }
                }

                {
                    // try owner
                    var packages = db.LookupByOwner(name).ToList();
                    if (packages.Count == 1)
                    {
                        return packages.First();
                    }
                    else if (packages.Count > 1)
                    {
                        // log results
                        foreach (var item in packages)
                        {
                            logger.Warning($"Multiple packages found for owner {name}:");
                            logger.Info(item.Id);
                        }
                    }
                }
            }

            return null;
        }

        private /*async*/ Task InstallPackageAsync(Package package)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var githubService = serviceProvider.GetRequiredService<IGitHubService>();

            logger.Info($"installing package {package.Id}...");

            //await githubService.InstallLatestReleaseAsync(package);
            _ = githubService.InstallLatestReleaseAsync(package);


            return Task.CompletedTask;
        }
    }
}
