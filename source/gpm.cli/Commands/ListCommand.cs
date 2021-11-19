using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using gpm.core.Services;
using gpm.core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            var library = LibraryUtil.LoadLibrary();

            logger.Success("Available packages:");
            //logger.Log("GitHub\tName\tInstalled Version");
            Console.WriteLine("Id\tUrl\tInstalled Version");
            foreach (var (key, package) in db.Packages)
            {
                var installedVersion = "";
                if (library.Plugins.ContainsKey(key))
                {
                    installedVersion = library.Plugins[key].InstalledVersion;
                }

                //logger.Info($"{package.ID}\t{package.Name}\t{installedVersion}");
                Console.WriteLine($"{key}\t{package.Url}\t{installedVersion}");
            }

        }

        #endregion Constructors
    }
}
