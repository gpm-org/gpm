using gpm.Core.Models;
using Octokit;

namespace gpm.Core.Services;

public interface IGitHubService
{
    /// <summary>
    /// Check if a release exists
    /// </summary>
    /// <param name="releases"></param>
    /// <param name="installedVersion"></param>
    /// <returns></returns>
    bool IsUpdateAvailable(IReadOnlyList<Release>? releases, string installedVersion);

    /// <summary>
    /// Check if a release exists
    /// </summary>
    /// <param name="installedVersion"></param>
    /// <returns></returns>
    Task<bool> IsUpdateAvailable(Package package, string installedVersion);

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

    /// <summary>
    /// Downloads a given release asset from GitHub and saves it to the cache location
    /// Creates a CacheManifest inside the library
    /// </summary>
    /// <param name="package"></param>
    /// <param name="asset"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    Task<bool> DownloadAssetToCache(Package package, ReleaseAsset asset, string version);
}
