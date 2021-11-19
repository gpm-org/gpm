using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using gpm.core.Models;
using gpm.core.Services;
using gpm.core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gpm.cli.Commands
{
    public class InstallCommand : Command
    {
        #region Fields

        private new const string Description = "";
        private new const string Name = "install";

        #endregion Fields

        #region Constructors

        public InstallCommand() : base(Name, Description)
        {
            AddArgument(new Argument<string[]>("names", "A package name or list of names. Can be a github repo url, a repo name or in the form of owner/name/id"));

            Handler = CommandHandler.Create<string[], IHost>(Action);
        }

        private void Action(string[] names, IHost host)
        {
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var db = serviceProvider.GetRequiredService<IDataBaseService>();

            if (names == null)
            {
                logger.Error("Please input a package name");
                return;
            }

            foreach (var name in names.Select(x => x.ToLower()))
            {
                var package = Parsepackagename(db, logger, name);

                if (package is not null)
                {
                    InstallPackage(logger, package);
                }
                else
                {
                    logger.Error($"package {name} not found");
                }
            }

        }

        private static Package? Parsepackagename(IDataBaseService db, ILoggerService logger, string name)
        {
            Package? package = null;
            if (Path.GetExtension(name) == ".git")
            {
                var packages = db.LookupByUrl(name).ToList();
                if (packages.Count == 1)
                {
                    package = packages.First();
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
                package = db.Lookup(id);
            }
            else if (name.Split('/').Length == 3)
            {
                var splits = name.Split('/');
                var id = $"{splits[0]}-{splits[1]}-{splits[2]}";
                package = db.Lookup(id);
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

        private static void InstallPackage(ILoggerService logger, Package package)
        {
            logger.Info($"installing package {package.Id}...");





        }
        #endregion Constructors
    }
}
