using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using gpm;
using gpm.Commands;

var rootCommand = new RootCommand
{
    new ListCommand(),
    new InstallCommand(),
    new UpdateCommand(),
    new RemoveCommand(),
    new InstalledCommand(),
    new UpgradeCommand()
};

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost(GenericHost.CreateHostBuilder)
    .Build();

// hack to get DI in system.commandline
parser.Invoke(new UpgradeCommand().Name);

parser.Invoke(args);
