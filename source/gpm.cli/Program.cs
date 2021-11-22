using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using gpm.cli;
using gpm.cli.Commands;
using Microsoft.Extensions.Hosting;

var rootCommand = new RootCommand
{
          new UpdateCommand(),
          new ListCommand(),
          new NewCommand(),
          new InstallCommand(),
          new RemoveCommand(),
};

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost(GenericHost.CreateHostBuilder)
    ;
var parser = builder.Build();

// hack to get DI in system.commandline
parser.Invoke(new UpdateCommand().Name);

parser.Invoke(args);
