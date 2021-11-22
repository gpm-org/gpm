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
    public class UpgradeCommand : Command
    {
        private new const string Description = "";
        private new const string Name = "upgrade";

        public UpgradeCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<IHost>(Action);
        }

        private void Action(IHost host)
        {
            var serviceProvider = host.Services;
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            // TODO: check if git is installed

            dataBaseService.FetchAndUpdateSelf();
        }
    }
}
