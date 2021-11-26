using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using gpm;
using gpm.Commands;

var rootCommand = new RootCommand
{
    new UpdateCommand(),
    new ListCommand(),
    new InstallCommand(),
    new RemoveCommand(),
    new InstalledCommand()
};

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost(GenericHost.CreateHostBuilder)
    .Build();

// TODO: hack to get DI in system.commandline
parser.Invoke(new UpgradeCommand().Name);

parser.Invoke(args);
