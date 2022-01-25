using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using gpm.Commands;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm;

public static class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var rootCommand = new RootCommand
                {
                    new SearchCommand(),
                    new InstallCommand(),
                    new UpdateCommand(),
                    new RemoveCommand(),
                    new ListCommand(),
                    new RestoreCommand(),
                    new UpgradeCommand(),
                    new NewCommand(),
                    new RunCommand()
                };

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseHost(CreateHostBuilder)
                .Build();

            return parser.Invoke(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(Startup.ConfigureAppConfiguration)
            .ConfigureServices(Startup.ConfigureServices)
            .UseSerilog(Startup.ConfigureSerilog);

}


