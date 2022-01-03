using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.core.Services;
using gpm.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands
{
    public class ListCommand : Command
    {
        private new const string Description = "Lists all installed packages.";
        private new const string Name = "list";

        public ListCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<IHost>(ListAction);
        }

        private void ListAction(IHost host)
        {
            var serviceProvider = host.Services;
            var taskService = serviceProvider.GetRequiredService<ITaskService>();
            taskService.List();
        }
    }
}
