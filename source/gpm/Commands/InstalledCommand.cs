using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
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

            Handler = CommandHandler.Create<string, string, IHost>(Installed.Action);
        }
    }
}
