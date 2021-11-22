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

namespace gpm_util.Commands
{
    public class UpdateCommand : Command
    {
        private new const string Description = "";
        private new const string Name = "update";

        public UpdateCommand() : base(Name, Description)
        {
            Handler = CommandHandler.Create<bool, IHost>(Action);
        }

        private void Action(bool all, IHost host)
        {
            var serviceProvider = host.Services;
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();



        }
    }
}
