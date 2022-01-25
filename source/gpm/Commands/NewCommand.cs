using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Text.Json.Serialization;
using gpm.Core.Models;
using gpm.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.CommandLine.NamingConventionBinder;

namespace gpm.Commands;

public class NewCommand : Command
{
    private new const string Description = "";
    private new const string Name = "new";

    public NewCommand() : base(Name, Description)
    {
        AddArgument(new Argument<string>("url", "github url."));

        AddOption(new Option<string>(new[] { "--id", "-i" }, "Additional identifier"));
        AddOption(new Option<string>(new[] { "--outdir", "-o" }, "Output directory"));

        Handler = CommandHandler.Create<string, string, string, IHost>(Action);
    }

    private async Task Action(string url, string id, string outdir, IHost host)
    {
        var serviceProvider = host.Services;
        var logger = serviceProvider.GetRequiredService<ILogger<NewCommand>>();
        var gitHubService = serviceProvider.GetRequiredService<IGitHubService>();

        // get additional info from github
        var (repo, topics) = await gitHubService.GetInfo(url);
        if (repo is null || topics is null)
        {
            logger.LogWarning("Could not get metadata from Github: {Url}", url);
            return;
        }

        // package
        var package = new Package(url)
        {
            Identifier = id,
            Description = repo.Description,
            Homepage = repo.Homepage,
            License = repo.License?.Name,
            Topics = topics.Names.ToArray()
        };

        // writing
        string outDirectory;
        if (string.IsNullOrEmpty(outdir))
        {
            outDirectory = Path.GetFullPath(Environment.CurrentDirectory);
        }
        else
        {
            var dir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outdir));
            if (Directory.Exists(dir))
            {
                outDirectory = dir;
            }
            else
            {
                Directory.CreateDirectory(dir);
                //logger.LogWarning("Output directory does not exist: {Outdir}", outdir);
                //return;
                outDirectory = dir;
            }
        }

        var filename = $"{repo.Name}@{id}".TrimEnd('@');
        var packageDbDir = Path.Combine(outDirectory, $"{repo.Owner?.Login}");
        Directory.CreateDirectory(packageDbDir);
        var path = Path.Combine(packageDbDir, $"{filename}.gpak");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonString = JsonSerializer.Serialize(package, options);
        await File.WriteAllTextAsync(path, jsonString);

        Log.Information("Created new package {Path}", path);
    }
}
