using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Commands;

public class RunCommand : Command
{
    private new const string Description = "Runs a globally installed gpm packages.";
    private new const string Name = "run";

    public RunCommand() : base(Name, Description)
    {
        Handler = CommandHandler.Create<IHost>(RunAction);
    }

    private void RunAction(IHost host)
    {
        var serviceProvider = host.Services;

        //TODO

    }
}
