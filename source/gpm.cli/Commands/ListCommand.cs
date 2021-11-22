using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using gpm.core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.cli.Commands
{
    public class ListCommand : Command
    {
        #region Fields

        private new const string Description = "";
        private new const string Name = "list";

        #endregion Fields

        #region Constructors

        public ListCommand() : base(Name, Description)
        {
            AddOption(new Option<string>(new[] { "--pattern", "-w" }, "Use optional search pattern (e.g. *.ink), if both regex and pattern is defined, pattern will be prioritized."));
            AddOption(new Option<string>(new[] { "--regex", "-r" }, "Use optional regex pattern."));

            Handler = CommandHandler.Create<string[], string, string, bool, bool, IHost>(Action);
        }

        private void Action(string[] path, string pattern, string regex, bool diff, bool list, IHost host)
        {
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var db = serviceProvider.GetRequiredService<IDataBaseService>();
            var library = serviceProvider.GetRequiredService<ILibraryService>();

            logger.Success("Available packages:");

            Console.WriteLine("Id\tUrl\tInstalled Version");
            foreach (var (key, package) in db.Packages)
            {
                var installedVersion = "";
                var model = library.Lookup(key);
                if (model.HasValue)
                {
                    installedVersion = model.Value.LastInstalledVersion;
                }


                Console.WriteLine($"{key}\t{package.Url}\t{installedVersion}");
            }

        }

        #endregion Constructors
    }
}
