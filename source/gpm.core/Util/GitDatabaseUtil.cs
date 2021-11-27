using System;
using System.Linq;
using gpm.core.Services;
using LibGit2Sharp;

namespace gpm.core.Util
{
    public static class GitDatabaseUtil
    {
        /// <summary>
        /// Fetch and Pull git database
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static MergeStatus? UpdateGitDatabase(ILoggerService logger)
        {
            //var commonSettings = settings.CommonSettings.Value;
            //ArgumentNullException.ThrowIfNull(commonSettings);

            // check if git is initialized
            try
            {
                using (new Repository(IAppSettings.GetGitDbFolder()))
                {
                }
                //commonSettings.IsInitialized = true;
                //settings.Save();
            }
            catch (RepositoryNotFoundException)
            {
                // git clone
                Repository.Clone(Constants.GPMDB, IAppSettings.GetGitDbFolder());
            }

            MergeStatus? status;
            using var repo = new Repository(IAppSettings.GetGitDbFolder());
            var statusItems = repo.RetrieveStatus(new StatusOptions());
            if (statusItems.Any())
            {
                throw new NotSupportedException("working tree not clean");
            }

            // fetch
            var logMessage = "";
            {
                var remote = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, remote.Name, refSpecs, null, logMessage);
            }
            logger.Debug(logMessage);

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
            var signature = new Signature(
                new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

            var result = Commands.Pull(repo, signature, options);
            if (result is not null)
            {
                status = result.Status;
                logger.Information($"Status: {status}");
            }
            else
            {
                throw new ArgumentNullException(nameof(result));
            }

            return status;
        }
    }
}
