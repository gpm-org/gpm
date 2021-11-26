using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gpm.core.Models;
using Octokit;

namespace gpm.core.Services
{
    public interface IGitHubService
    {
        /// <summary>
        /// Download and install an asset file from a Github repo.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="releases"></param>
        /// <param name="requestedVersion"></param>
        /// <param name="slot"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        Task<bool> InstallReleaseAsync(
            Package package,
            IReadOnlyList<Release> releases,
            string? requestedVersion,
            int slot = 0);

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
    }
}
