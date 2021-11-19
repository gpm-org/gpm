using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Threading.Tasks;
using gpm.core;
using gpm.core.Models;
using gpm.core.Services;
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
            var serviceProvider = host.Services;
            var settings = serviceProvider.GetRequiredService<IAppSettings>();
            var logger = serviceProvider.GetRequiredService<ILoggerService>();

            // TODO: check for git

            MergeStatus? status = null;
            // update database
            status = UpdateGitDatabase(logger, settings);


            // init database
            if (!File.Exists(AppSettings.GetDbFile()))
            {
                UpdateDatabase(logger);
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
                        UpdateDatabase(logger);
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(status));
                }
            }

            // Update packages



        }

        private static void UpdateDatabase(ILoggerService logger)
        {
            var files = Directory.GetFiles(AppSettings.GetGitDbFolder(), "*.gpak", SearchOption.AllDirectories);
            var packages = new List<Package>();
            foreach (var file in files)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                Package? package;
                try
                {
                    package = JsonSerializer.Deserialize<Package>(File.ReadAllText(file));
                }
                catch (Exception e)
                {
                    package = null;
                    logger.Error(e);
                }
                
                if (package is not null)
                {
                    packages.Add(package);
                }
            }

            try
            {
                using var file = File.Create(AppSettings.GetDbFile());
                Serializer.Serialize(file, packages);
            }
            catch (Exception)
            {
                throw;
            }

            logger.Success("Database updated");
        }

        private static MergeStatus? UpdateGitDatabase(ILoggerService logger, IAppSettings settings)
        {
            var commonSettings = settings.CommonSettings?.Value;

            if (commonSettings is null)
            {
                throw new ArgumentException(nameof(CommonSettings));
            }

            // check if git is initialized
            try
            {
                using (var repo = new Repository(AppSettings.GetGitDbFolder()))
                {

                }
                commonSettings.IsInitialized = true;
                settings.Save();
            }
            catch (LibGit2Sharp.RepositoryNotFoundException)
            {
                // git clone
                try
                {
                    var s = Repository.Clone(Constants.GPMDB, AppSettings.GetGitDbFolder());
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }

            MergeStatus? status;
            using (var repo = new Repository(AppSettings.GetGitDbFolder()))
            {
                // TODO: check if the repo was changed
                var statusItems = repo.RetrieveStatus(new LibGit2Sharp.StatusOptions());
                if (statusItems.Any())
                {
                    throw new NotImplementedException("working tree not clean");
                }

                // fetch
                var logMessage = "";
                {
                    var remote = repo.Network.Remotes["origin"];
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    LibGit2Sharp.Commands.Fetch(repo, remote.Name, refSpecs, null, logMessage);
                }
                logger.Log(logMessage);

                // Pull -ff
                var options = new PullOptions()
                {
                    MergeOptions = new MergeOptions()
                    {
                        FailOnConflict = true,
                        FastForwardStrategy = FastForwardStrategy.FastForwardOnly,
                    }
                };
                // TODO dummy user information to create a merge commit
                var signature = new LibGit2Sharp.Signature(
                    new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

                var result = LibGit2Sharp.Commands.Pull(repo, signature, options);
                if (result is not null)
                {
                    status = result.Status;
                    logger.Info($"Status: {status}");
                }
                else
                {
                    throw new ArgumentNullException(nameof(result));
                }
            }

            return status;
        }

        #endregion Constructors
    }
}
