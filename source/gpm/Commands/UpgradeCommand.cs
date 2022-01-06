using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Core.Services;
using gpm.Core.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class UpgradeCommand : Command
    {
        private new const string Description = "Update the local package registry.";
        private new const string Name = "upgrade";

        public UpgradeCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<IHost>(Upgrade);
        }

        private void Upgrade(IHost host)
        {
            var serviceProvider = host.Services;
            var taskService = serviceProvider.GetRequiredService<ITaskService>();

            taskService.Upgrade();
        }
    }
}
