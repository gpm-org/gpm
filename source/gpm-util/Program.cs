﻿// See https://aka.ms/new-console-template for more information

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using gpm_util;
using gpm_util.Commands;

var rootCommand = new RootCommand
{
    new UpdateCommand(),
    new NewCommand(),
};

var builder = new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseHost(GenericHost.CreateHostBuilder)
    ;
var parser = builder.Build();

// hack to get DI in system.commandline
parser.Invoke(new UpgradeCommand().Name);

parser.Invoke(args);

