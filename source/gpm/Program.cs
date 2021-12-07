using System;
using Serilog;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.IO;
using gpm;
using gpm.Commands;
using gpm.core.Services;

var rootCommand = new RootCommand
{
    new SearchCommand(),
    new InstallCommand(),
    new UpdateCommand(),
    new RemoveCommand(),
    new ListCommand(),
    new RestoreCommand(),
    new UpgradeCommand(),
    new NewCommand()
};

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(IAppSettings.GetLogsFolder(), "gpm-log.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost(GenericHost.CreateHostBuilder)
    .Build();

#if DEBUG
Environment.CurrentDirectory = GetTestSlot();
static string GetTestSlot()
{
    var folder = Path.Combine(IAppSettings.GetAppDataFolder(),
        "TESTSLOT"
    );
    if (!Directory.Exists(folder))
    {
        Directory.CreateDirectory(folder);
    }
    return folder;
}
#endif

parser.Invoke(args);


