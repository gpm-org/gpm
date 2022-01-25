using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using gpm.Core.Services;
using gpm.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands;

public class RunCommand : Command
{
    private new const string Description = "Runs a globally installed gpm package.";
    private new const string Name = "run";

    public RunCommand() : base(Name, Description)
    {
        AddArgument(new Argument<string>("name",
            "The package name. Can be a github repo url, a repo name or in the form of owner/name/id. "));

        AddOption(new Option<string[]?>(new[] { "--args", "-a" },
            "Commandline arguments."));

        Handler = CommandHandler.Create<string, string[]?, IHost>(RunAction);
    }

    private async Task RunAction(string name, string[]? args, IHost host)
    {
        var serviceProvider = host.Services;
        var taskService = serviceProvider.GetRequiredService<ITaskService>();

        await taskService.UpgradeAndRun(name, args);
    }
}
