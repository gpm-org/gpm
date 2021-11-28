using System.Collections.Generic;
using System.Threading.Tasks;
using gpm.core.Models;
using Octokit;

namespace gpm.core.Services
{
    public interface IGitHubService
    {
        /// <summary>
        /// Check if a release exists that
        /// </summary>
        /// <param name="package"></param>
        /// <param name="releases"></param>
        /// <param name="installedVersion"></param>
        /// <returns></returns>
        bool IsUpdateAvailable(Package package, IReadOnlyList<Release>? releases, string installedVersion);

        /// <summary>
        /// Fetch all github releases for a given package.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        Task<IReadOnlyList<Release>?> GetReleasesForPackage(Package package);

        /// <summary>
        /// Get metadata for a github repo
        /// </summary>
        /// <param name="url"></param>
        Task<(Repository? repo, RepositoryTopics? topics)> GetInfo(string url);
    }
}
