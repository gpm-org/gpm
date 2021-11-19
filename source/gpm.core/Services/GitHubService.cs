using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using gpm.core.Models;
using Octokit;

namespace gpm.core.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client = new(new ProductHeaderValue("RedCommunityToolkit"));

        private static readonly HttpClient s_client = new();

        public GitHubService()
        {

        }

        public async Task<bool> InstallLatestReleaseAsync(Package model)
        {
            IReadOnlyList<Release>? releases;
            try
            {
                releases = await _client.Repository.Release.GetAll(model.RepoOwner, model.RepoName);
            }
            catch (Exception)
            {
                throw;
            }
            if (releases == null || !releases.Any())
            {
                return false;
            }


            // install latest
            var latest = releases[0];
            var idx = model.AssetIndex;

            var zip = latest.Assets[idx];
            if (zip is null)
            {
                return false;
            }
            var url = latest.Assets[idx].BrowserDownloadUrl;
            var filename = latest.Assets[idx].Name;

            await DownloadAsset(model, url, filename);



            // udpate current info in local db
            //var version = latest.TagName;
            //model.InstalledVersion = version;
            //if (model.InstalledVersions is null)
            //{
            //    model.InstalledVersions = new();
            //}
            //model.InstalledVersions.Add(version, filename);

            return true;

        }

        private static async Task DownloadAsset(Package model, string url, string filename)
        {
            try
            {
                var libdir = Path.Combine(IAppSettings.GetCacheFolder(), $"{model.Id}");
                if (!Directory.Exists(libdir))
                {
                    Directory.CreateDirectory(libdir);
                }
                var path = Path.Combine(libdir, filename);

                var uri = new Uri(url);
                var response = await s_client.GetAsync(uri);

                response.EnsureSuccessStatusCode();

                using var fs = new FileStream(path, System.IO.FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fs);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }


        //    private async Task DownloadUpdateAsync(Manifest manifest)
        //    {
        //        var latestVersion = manifest.Version;

        //        if (!b)
        //        {
        //            return true;
        //        }

        //        using (var wc = new WebClient())
        //        {
        //            var dlObservable = Observable.FromEventPattern<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
        //                handler => wc.DownloadProgressChanged += handler,
        //                handler => wc.DownloadProgressChanged -= handler);
        //            var dlCompleteObservable = Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>(
        //                handler => wc.DownloadFileCompleted += handler,
        //                handler => wc.DownloadFileCompleted -= handler);

        //            _ = dlObservable
        //                .Select(_ => (double)_.EventArgs.ProgressPercentage)
        //                .Subscribe(d =>
        //                {
        //                    Report(d / 100);
        //                });

        //            _ = dlCompleteObservable
        //                .Select(_ => _.EventArgs)
        //                .Subscribe(c =>
        //                {
        //                    OnDownloadCompletedCallback(c, manifest, type);
        //                });

        //            var uri = new Uri($"{GetUpdateUri().TrimEnd('/')}/{manifest.Get(type).Key}");
        //            var physicalPath = Path.Combine(Path.GetTempPath(), manifest.Get(type).Key);
        //            wc.DownloadFileAsync(uri, physicalPath);
        //        }
        //        await Task.CompletedTask;
        //    }

        //    private void OnDownloadCompletedCallback(AsyncCompletedEventArgs e, Manifest manifest, EIncludedFiles type)
        //    {
        //        if (e.Cancelled)
        //        {
        //            Console.WriteLine("File download cancelled.");
        //        }

        //        if (e.Error != null)
        //        {
        //            Console.WriteLine(e.Error);
        //        }

        //        // check downloaded file
        //        var physicalPath = new FileInfo(Path.Combine(Path.GetTempPath(), manifest.Get(type).Key));
        //        if (physicalPath.Exists)
        //        {
        //            using (var mySha256 = SHA256.Create())
        //            {
        //                var hash = Helpers.HashFile(physicalPath, mySha256);
        //                if (manifest.Get(type).Value.Equals(hash))
        //                {
        //                    HandleUpdateFromFile(physicalPath);
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Downloaded file does not match expected file.");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("File download failed.");
        //        }
        //    }
        //
    }
}
