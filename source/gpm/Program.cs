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
    //new NewCommand(),
    new InstallCommand(),
    new RemoveCommand(),
    new InstalledCommand()
};

var builder = new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseHost(GenericHost.CreateHostBuilder)
    ;
var parser = builder.Build();

// TODO: hack to get DI in system.commandline
parser.Invoke(new UpgradeCommand().Name);

parser.Invoke(args);
