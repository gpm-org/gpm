using gpm.Core.Models;
using gpm.Core.Util;

namespace gpm.Core.Services;

public interface IGitHubService
{
    /// <summary>
    /// Fetch all github releases for a given package.
    /// 1 API call
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    Task<IEnumerable<ReleaseModel>?> GetReleasesForPackage(Package package);

    /// <summary>
    /// Get metadata for a github repo
    /// 1 API call
    /// </summary>
    /// <param name="url"></param>
    Task<(Octokit.Repository? repo, RepositoryTopics? topics)> GetInfo(string url);

    /// <summary>
    /// Check if a release exists and returns the releases
    /// 1 API call
    /// </summary>
    /// <param name="package"></param>
    /// <param name="requestedVersion"></param>
    /// <returns></returns>
    //Task<IReadOnlyList<ReleaseModel>?> IsUpdateAvailable(Package package, string requestedVersion);
    Task<AsyncOut<bool, ReleaseModel?>> TryGetRelease(Package package, string requestedVersion);

    /// <summary>
    /// Downloads a given release asset from GitHub and saves it to the cache location
    /// Creates a CacheManifest inside the library
    /// </summary>
    /// <param name="package"></param>
    /// <param name="asset"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    Task<bool> DownloadAssetToCache(Package package, ReleaseAssetModel asset, string version);

}
