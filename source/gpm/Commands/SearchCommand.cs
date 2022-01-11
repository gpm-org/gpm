using System.CommandLine;
using System.CommandLine.Invocation;
using gpm.Core.Extensions;
using gpm.Core.Models;
using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.CommandLine.NamingConventionBinder;

namespace gpm.Commands;

public class SearchCommand : Command
{
    private new const string Description = "Search packages in the gpm registry.";
    private new const string Name = "search";

    public SearchCommand() : base(Name, Description)
    {
        AddArgument(new Argument<string>("pattern",
            () => "*",
            "Filter the available packages by a pattern. E.g. `wolven*`"));

        Handler = CommandHandler.Create<string, IHost>(Action);
    }

    private void Action(string pattern, IHost host)
    {
        var serviceProvider = host.Services;
        var taskService = serviceProvider.GetRequiredService<ITaskService>();
        var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

        // some QoL default cases
        if (string.IsNullOrEmpty(pattern))
        {
            pattern = "*";
        }

        // update here
        taskService.Upgrade();

        // check search pattern then regex
        IEnumerable<Package> available;
        if (string.IsNullOrEmpty(pattern))
        {
            available = dataBaseService.Values;
        }
        else
        {
            var matches = dataBaseService.Values
                .Select(x => x.Name)
                .MatchesWildcard(x => x, pattern);

            available = dataBaseService.Values.Where(x => matches.Contains(x.Name)).ToList();
        }

        Log.Information("Available packages:");
        Console.WriteLine("Id\tUrl");
        foreach (var package in available)
        {
            Console.WriteLine("{0}\t{1}", package.Id, package.Url);
            //Log.Information("{Key}\t{Package}", key, package.Url);
        }
    }


}
