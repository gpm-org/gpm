// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using gpm_util;
using gpm_util.Commands;
using Serilog;

var rootCommand = new RootCommand
{
    new UpdateCommand(),
    new NewCommand(),
};

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost(GenericHost.CreateHostBuilder).Build();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

// hack to get DI in system.commandline
parser.Invoke(new UpgradeCommand().Name);

parser.Invoke(args);

