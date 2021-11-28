using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Nito.AsyncEx;
using System.Threading.Tasks;
using gpm.core.Extensions;
using gpm.core.Models;
using Octokit;
using gpm.core.Util;
using Serilog;

namespace gpm.core.Services
{
    /// <summary>
    /// A service class to handle requests to github
    /// </summary>
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _gitHubClient = new(new ProductHeaderValue("gpm"));

        private readonly AsyncLock _loadingLock = new();

        /// <summary>
        /// Fetch all github releases for a given package.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Release>?> GetReleasesForPackage(Package package)
        {
            using var ssc = new ScopedStopwatch();

            IReadOnlyList<Release>? releases;

            // get releases from github repo
            using (await _loadingLock.LockAsync())
            {
                try
                {
                    releases = await _gitHubClient.Repository.Release.GetAll(package.RepoOwner, package.RepoName);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Fetching github releases failed");
                    releases = null;
                }
            }

            return releases;
        }

        /// <summary>
        /// Get metadata for a github repo
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

        /// <summary>
        /// Check if a release exists that
        /// </summary>
        /// <param name="package"></param>
        /// <param name="releases"></param>
        /// <param name="installedVersion"></param>
        /// <returns></returns>
        public bool IsUpdateAvailable(Package package, IReadOnlyList<Release>? releases, string installedVersion)
        {
            using var ssc = new ScopedStopwatch();

            if (releases is null || !releases.Any())
            {
                Log.Warning("No releases found for package {Package}", package);
                return false;
            }
            if (!releases.Any(x => x.TagName.Equals(installedVersion)))
            {
                Log.Warning("No releases found with version {InstalledVersion} for package {Package}",
                    installedVersion, package);
                return false;
            }

            // idx 0 is latest release
            if (!releases[0].TagName.Equals(installedVersion))
            {
                return true;
            }

            Log.Information($"Latest release already installed");
            return false;
        }


        /// <summary>
        /// !!! REMOVE WHEN OCTOKIT UPDATES FROM V0.50 !!!
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
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
    }
}
