using Serilog;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using gpm;
using gpm.Commands;
using gpm.core.Services;

var rootCommand = new RootCommand
{
    new ListCommand(),
    new InstallCommand(),
    new UpdateCommand(),
    new RemoveCommand(),
    new InstalledCommand(),
    new UpgradeCommand()
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(IAppSettings.GetLogsFolder(), rollingInterval: RollingInterval.Day)
    .CreateLogger();

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost(GenericHost.CreateHostBuilder)
    .Build();

// hack to get DI in system.commandline
parser.Invoke(new UpgradeCommand().Name);

parser.Invoke(args);
