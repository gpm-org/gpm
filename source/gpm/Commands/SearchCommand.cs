using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Commands
{
    public class SearchCommand : Command
    {
        private new const string Description = "Search packages in the gpm registry.";
        private new const string Name = "search";

        public SearchCommand() : base(Name, Description)
        {
            AddOption(new Option<string>(new[] { "--pattern", "-w" },
                "Use optional search pattern (e.g. *.ink), if both regex and pattern is defined, pattern will be prioritized."));
            AddOption(new Option<string>(new[] { "--regex", "-r" }, "Use optional regex pattern."));

            Handler = CommandHandler.Create<string, string, IHost>(Action);
        }

        private void Action(string pattern, string regex, IHost host)
        {
            var serviceProvider = host.Services;
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            // update here
            Upgrade.Action(host);

            Log.Information("Available packages:");
            Console.WriteLine("Id\tUrl");
            foreach (var (key, package) in dataBaseService)
            {
                Console.WriteLine("{0}\t{1}", key, package.Url);
                //Log.Information("{Key}\t{Package}", key, package.Url);
            }
        }


    }
}
