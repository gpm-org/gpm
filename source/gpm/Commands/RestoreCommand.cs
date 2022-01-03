using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class RestoreCommand : Command
    {
        private new const string Description = "Restore packages defined in the local package lock file.";
        private new const string Name = "restore";

        public RestoreCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<IHost>(UpdateAndRestore);
        }

        private void UpdateAndRestore(IHost host)
        {
            var serviceProvider = host.Services;
            var taskService = serviceProvider.GetRequiredService<ITaskService>();

            taskService.UpdateAndRestore();
        }
    }
}
