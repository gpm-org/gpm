using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using gpm.core.Services;
using gpm.core.Util;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit;

namespace gpm.Commands
{
    public class UpdateCommand : Command
    {
        private new const string Description = "";
        private new const string Name = "update";

        public UpdateCommand() : base(Name, Description)
        {
            AddOption(new Option<string[]>(new[] { "--packages", "-p" }, ""));
            AddOption(new Option<bool>(new[] { "--all", "-a" }, ""));

            Handler = CommandHandler.Create<string[], bool, bool, IHost>(Action);
        }

        private void Action(string[] packages, bool self, bool all, IHost host)
        {
            var serviceProvider = host.Services;
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            // Update packages
            if (packages != null)
            {
            }

            // TODO
        }
    }
}