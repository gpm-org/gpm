using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.cli.Commands
{
    public class InstalledCommand : Command
    {
        private new const string Description = "";
        private new const string Name = "installed";

        public InstalledCommand() : base(Name, Description)
        {
            AddOption(new Option<string>(new[] { "--pattern", "-w" }, "Use optional search pattern (e.g. *.ink), if both regex and pattern is defined, pattern will be prioritized."));
            AddOption(new Option<string>(new[] { "--regex", "-r" }, "Use optional regex pattern."));

            Handler = CommandHandler.Create<string, string, IHost>(Action);
        }

        private void Action(string pattern, string regex, IHost host)
        {
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var library = serviceProvider.GetRequiredService<ILibraryService>();

            logger.Success("Installed packages:");

            //Console.WriteLine("Id\tUrl\tInstalled Version");

            foreach (var package in library.GetPackages())
            {
                // get info from db?

                var versions = new List<string>();
                foreach (var (version, manifest) in package.Manifests)
                {
                    if (manifest.DeployManifest is not null)
                    {
                        versions.Add(version);
                    }
                }

                // print results
                if (versions.Count > 0)
                {
                    Console.WriteLine($"{package.Key}");
                    foreach (var v in versions)
                    {
                        Console.WriteLine($"{v}");
                    }
                }
            }
        }
    }
}
