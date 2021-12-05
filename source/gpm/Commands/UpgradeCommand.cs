using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Tasks;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class UpgradeCommand : Command
    {
        private new const string Description = "Update the local package registry.";
        private new const string Name = "upgrade";

        public UpgradeCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<IHost>(Upgrade.Action);
        }
    }
}
