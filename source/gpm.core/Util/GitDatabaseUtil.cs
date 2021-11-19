using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gpm.core.Services;
using LibGit2Sharp;

namespace gpm.core.Util
{
    public static class GitDatabaseUtil
    {
        public static MergeStatus? UpdateGitDatabase(ILoggerService logger, IAppSettings settings)
        {
            var commonSettings = settings.CommonSettings?.Value;

            if (commonSettings is null)
            {
                throw new ArgumentException(nameof(CommonSettings));
            }

            // check if git is initialized
            try
            {
                using (var repo = new Repository(IAppSettings.GetGitDbFolder()))
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
                    var s = Repository.Clone(Constants.GPMDB, IAppSettings.GetGitDbFolder());
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
            using (var repo = new Repository(IAppSettings.GetGitDbFolder()))
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
    }
}
