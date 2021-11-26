using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class InstalledCommand : Command
    {
        private new const string Description = "Lists all installed packages.";
        private new const string Name = "installed";

        public InstalledCommand() : base(Name, Description)
        {
            AddOption(new Option<string>(new[] { "--pattern", "-w" },
                "Use optional search pattern (e.g. *.ink), if both regex and pattern is defined, pattern will be prioritized."));
            AddOption(new Option<string>(new[] { "--regex", "-r" }, "Use optional regex pattern."));

            Handler = CommandHandler.Create<string, string, IHost>(Action);
        }

        private void Action(string pattern, string regex, IHost host)
        {
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var library = serviceProvider.GetRequiredService<ILibraryService>();

            logger.Success("Installed packages:");

            foreach (var (key, model) in library)
            {
                if (library.IsInstalled(key))
                {

                    foreach (var (_, manifest) in model.Slots)
                    {
                        // print installed slots
                        Console.WriteLine($"{manifest.Version}\t{manifest.FullPath}");
                    }
                }
            }
        }
    }
}
