using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class ListCommand : Command
    {
        private new const string Description = "Lists all available packages.";
        private new const string Name = "list";

        public ListCommand() : base(Name, Description)
        {
            AddOption(new Option<string>(new[] { "--pattern", "-w" },
                "Use optional search pattern (e.g. *.ink), if both regex and pattern is defined, pattern will be prioritized."));
            AddOption(new Option<string>(new[] { "--regex", "-r" }, "Use optional regex pattern."));

            Handler = CommandHandler.Create<string, string, IHost>(Action);
        }

        private void Action(string pattern, string regex, IHost host)
        {
            var serviceProvider = host.Services;
            var db = serviceProvider.GetRequiredService<IDataBaseService>();

            db.ListAllPackages();
        }


    }
}
