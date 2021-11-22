using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using gpm.core;
using gpm.core.Models;
using gpm.core.Services;
using gpm.core.Util;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace gpm.cli.Commands
{
    public class UpdateCommand : Command
    {
        #region Fields

        private new const string Description = "";
        private new const string Name = "update";

        #endregion Fields

        #region Constructors

        public UpdateCommand() : base(Name, Description)
        {
            AddOption(new Option<string[]>(new[] { "--packages", "-p" }, ""));
            //AddOption(new Option<bool>(new[] { "--self", "-s" }, ""));
            AddOption(new Option<bool>(new[] { "--all", "-a" }, ""));

            Handler = CommandHandler.Create<string[], bool, bool, IHost>(Action);
        }

        private void Action(string[] packages, bool self, bool all, IHost host)
        {
            Task.Run(async () =>
            {
                Octokit.GitHubClient _client = new(new Octokit.ProductHeaderValue("gpm"));
                var releases = await _client.Repository.Release.GetAll("wolvenkit", "wolvenkit");
            });

            var serviceProvider = host.Services;
            var settings = serviceProvider.GetRequiredService<IAppSettings>();
            var logger = serviceProvider.GetRequiredService<ILoggerService>();

            // TODO: check for git

            MergeStatus? status = null;
            // update database
            status = GitDatabaseUtil.UpdateGitDatabase(logger, settings);

            // init database
            if (!File.Exists(IAppSettings.GetDbFile()))
            {
                DatabaseUtil.UpdateDatabase(logger);
            }
            else
            {
                if (status.HasValue)
                {
                    if (status.Value == MergeStatus.UpToDate)
                    {
                        // do nothing
                    }
                    else if (status.Value == MergeStatus.FastForward)
                    {
                        // re-create database
                        DatabaseUtil.UpdateDatabase(logger);
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(status));
                }
            }

            // Update packages
            if (packages != null)
            {

            }


            // TODO

        }





        #endregion Constructors
    }
}
