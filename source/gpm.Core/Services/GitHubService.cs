using gpm.Core.Exceptions;
using gpm.Core.Extensions;
using gpm.Core.Models;
using gpm.Core.Util;
using gpm.Core.Util.Builders;
using Nito.AsyncEx;
using Octokit;
using Serilog;

namespace gpm.Core.Services;

/// <summary>
/// A service class to handle requests to github
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly ILibraryService _libraryService;

    private readonly GitHubClient _gitHubClient = new(new ProductHeaderValue("gpm"));
    private static readonly HttpClient s_client = new();
    private readonly AsyncLock _loadingLock = new();

    public GitHubService(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    /// <summary>
    /// Fetch all github releases for a given package.
    /// 1 API call
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ReleaseModel>?> GetReleasesForPackage(Package package)
    {
        using var ssc = new ScopedStopwatch();

        // get releases from github repo
        using (await _loadingLock.LockAsync())
        {
            try
            {
                var releases = await _gitHubClient.Repository.Release.GetAll(package.Owner, package.Name);
                return releases?.Select(x => new ReleaseModel(x));
            }
            catch (Exception e)
            {
                Log.Error(e, "Fetching github releases failed");
                return null;
            }
        }
    }

    /// <summary>
    /// Get metadata for a github repo
    /// 2 API calls
    /// </summary>
    /// <param name="url"></param>
    public async Task<(Repository? repo, RepositoryTopics? topics)> GetInfo(string url)
    {
        using var sw = new ScopedStopwatch();

        var fi = new FileInfo(url);
        if (fi.Directory is null)
        {
            throw new ArgumentException(nameof(url));
        }
        var rOwner = fi.Directory.Name;
        var rName = fi.Name.Split('.').First();
        if (string.IsNullOrEmpty(rOwner) || string.IsNullOrEmpty(rName))
        {
            throw new ArgumentException(nameof(url));
        }

        try
        {
            var repo = await _gitHubClient.Repository.Get(rOwner, rName);
            var topics = await GetAllTopics(repo.Id);
            return (repo, topics);
        }
        catch (HttpRequestException httpRequestException)
        {
            Log.Error(httpRequestException, "Downloading asset from {Url} failed", url);
            return (null, null);
        }
        catch (Exception e)
        {
            Log.Error(e, "Downloading asset from {Url} failed", url);
            return (null, null);
        }
    }

    //!!! REMOVE WHEN OCTOKIT UPDATES FROM V0.50 !!!
    // 1 API call
    [ManualRoute("GET", "/repositories/{id}/topics")]
    private async Task<RepositoryTopics> GetAllTopics(long repositoryId)
    {
        var endpoint = "repositories/{0}/topics".FormatUri(repositoryId);
        var ac = new ApiConnection(_gitHubClient.Connection);
        var data = await ac
            .Get<RepositoryTopics>(endpoint, null, "application/vnd.github.mercy-preview+json")
            .ConfigureAwait(false);

        return data ?? new RepositoryTopics();
    }

    /// <summary>
    /// Check if a release exists and returns the releases
    /// 1 API call
    /// </summary>
    /// <param name="package"></param>
    /// <param name="requestedVersion">the requested version of the package. leave null or empty for the latest release</param>
    /// <returns></returns>
    public async Task<AsyncOut<bool, ReleaseModel?>> TryGetRelease(Package package, string requestedVersion)
    {
        var releases = await GetReleasesForPackage(package);
        if (releases is null)
        {
            Log.Warning("[{Package}] No releases found for package", package);
            return (false, null);
        }

        var releaseModels = releases.ToList();
        if (!releaseModels.Any())
        {
            Log.Warning("[{Package}] No releases found for package", package);
            return (false, null);
        }

        if (!ReleasesContainTag(releaseModels, requestedVersion))
        {
            Log.Warning("[{Package}] No releases found for package", package);
            return (false, null);
        }

        // get correct release
        var release = string.IsNullOrEmpty(requestedVersion)
            ? releaseModels.First() //latest
            : releaseModels.FirstOrDefault(x => x.TagName.Equals(requestedVersion));

        if (release == null)
        {
            Log.Warning("No release found for version {RequestedVersion}", requestedVersion);
            return (false, null);
        }

        return (true, release);

        // LOCAL FUNCTION
        static bool ReleasesContainTag(IEnumerable<ReleaseModel>? releases, string version)
        {
            using var ssc = new ScopedStopwatch();

            if (string.IsNullOrEmpty(version))
            {
                return true;
            }

            if (releases is null)
            {
                return false;
            }

            var releaseModels = releases.ToList();
            if (!releaseModels.Any())
            {
                return false;
            }
            if (!releaseModels.Any(x => x.TagName.Equals(version)))
            {
                return false;
            }

            // idx 0 is latest release
            if (releaseModels.First().TagName.Equals(version))
            {
                Log.Information("Latest release already installed");
                return false;
            }

            return true;
        }
    }


    /// <summary>
    /// Downloads a given release asset from GitHub and saves it to the cache location
    /// Creates a CacheManifest inside the library
    /// </summary>
    /// <param name="package"></param>
    /// <param name="release"></param>
    /// <returns></returns>
    public async Task<bool> DownloadAssetToCache(Package package, ReleaseModel release)
    {
        using var ssc = new ScopedStopwatch();

        // get correct release asset
        // TODO support multiple asset files?
        var assets = release.Assets;
        ArgumentNullException.ThrowIfNull(assets);
        var assetBuilder = IPackageBuilder.CreateDefaultBuilder<AssetBuilder>(package);
        var asset = assetBuilder.Build(release.Assets);
        if (asset is null)
        {
            Log.Warning("No release asset found for version {RequestedVersion}",
                release.TagName);
            return false;
        }

        var releaseFilename = asset.Name;
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseFilename);

        var packageCacheFolder = Path.Combine(IAppSettings.GetCacheFolder(), $"{package.Id}", $"{release.TagName}");
        var assetCacheFile = Path.Combine(packageCacheFolder, releaseFilename);

        // check if already exists
        if (CheckIfCachedFileExists())
        {
            Log.Information("Asset exists in cache: {AssetCacheFile}. Using cached file",
                assetCacheFile);
            return true;
        }

        var url = asset.BrowserDownloadUrl;
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(url);

        try
        {
            Log.Information("Downloading asset from {Url} ...", url);

            var response = await s_client.GetAsync(new Uri(url));
            response.EnsureSuccessStatusCode();

            if (!Directory.Exists(packageCacheFolder))
            {
                Directory.CreateDirectory(packageCacheFolder);
            }

            await using var ms = new MemoryStream();
            await response.Content.CopyToAsync(ms);

            // sha and size the ms
            var size = ms.Length;
            var sha = HashUtil.Sha512Bytes(ms);

            // write to file
            ms.Seek(0, SeekOrigin.Begin);
            await using var fs = new FileStream(assetCacheFile, System.IO.FileMode.Create, FileAccess.Write);
            await ms.CopyToAsync(fs);

            Log.Information("Downloaded asset {ReleaseFilename} with hash {Hash}", releaseFilename,
                HashUtil.BytesToString(sha));
            Log.Information("Saving file to local cache: {AssetCacheFile}", assetCacheFile);

            // cache manifest
            var cacheManifest = new CacheManifest()
            {
                Files = new[]
                {
                        new HashedFile(releaseFilename, sha, size)
                    }
            };

            var model = _libraryService.GetOrAdd(package);
            model.CacheData.AddOrUpdate(release.TagName, cacheManifest);
            _libraryService.Save();

            return true;
        }
        catch (HttpRequestException httpRequestException)
        {
            Log.Error(httpRequestException, "Downloading asset from {Url} failed", url);
            return false;
        }
        catch (Exception e)
        {
            Log.Error(e, "Downloading asset from {Url} failed", url);
            return false;
        }

        bool CheckIfCachedFileExists()
        {
            if (!File.Exists(assetCacheFile))
            {
                return false;
            }

            if (!_libraryService.TryGetValue(package.Id, out var existingModel))
            {
                return false;
            }

            var cacheManifest = existingModel.CacheData.GetOptional(release.TagName);
            if (!cacheManifest.HasValue)
            {
                return false;
            }
            if (cacheManifest.Value.Files is null)
            {
                return false;
            }

            // if the cache manifest contains a file with this name
            var fileInCache = cacheManifest.Value.Files
                .FirstOrDefault(x => Path.Combine(packageCacheFolder, x.Name.NotNullOrEmpty()).Equals(assetCacheFile));
            if (fileInCache is { })
            {
                // size and hash
                using var fs = new FileStream(assetCacheFile, System.IO.FileMode.Open, FileAccess.Read);
                var size = fs.Length;
                var sha = HashUtil.Sha512Bytes(fs);
                if (fileInCache.Sha512 != null && fileInCache.Sha512.SequenceEqual(sha) && fileInCache.Size == size)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
