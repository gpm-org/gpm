using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using gpm.cli;
using gpm.cli.Commands;

var rootCommand = new RootCommand
{
          new ListCommand(),

};

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost(GenericHost.CreateHostBuilder)
    .Build();

parser.Invoke(args);
