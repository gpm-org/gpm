using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine.NamingConventionBinder;

namespace gpm.Commands;

public class RestoreCommand : Command
{
    private new const string Description = "Restore packages defined in the local package lock file.";
    private new const string Name = "restore";

    public RestoreCommand() : base(Name, Description)
    {
        Handler = CommandHandler.Create<IHost>(UpdateAndRestore);
    }

    private async Task UpdateAndRestore(IHost host)
    {
        var serviceProvider = host.Services;
        var taskService = serviceProvider.GetRequiredService<ITaskService>();

        await taskService.UpgradeAndRestore();
    }
}
