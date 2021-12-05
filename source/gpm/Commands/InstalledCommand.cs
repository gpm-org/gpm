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
            Handler = CommandHandler.Create<IHost>(Installed.Action);
        }
    }
}
