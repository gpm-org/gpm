using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using RedCommunityToolkit.Models;
using Windows.Storage;

namespace RedCommunityToolkit.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client = new(new ProductHeaderValue("RedCommunityToolkit"));

        private static readonly HttpClient client = new HttpClient();

        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        public GitHubService()
        {

        }


        public async Task<bool> InstallLatestReleaseAsync(PluginModel model)
        {
            var fi = new FileInfo(model.ID);
            if (fi.Directory is null)
            {
                throw new ArgumentException(nameof(model.ID));
            }
            var rOwner = fi.Directory.Name;
            var rName = fi.Name.Split('.').First();

            if (string.IsNullOrEmpty(rOwner) || string.IsNullOrEmpty(rName))
            {
                throw new ArgumentException(nameof(model.ID));
            }

            IReadOnlyList<Release>? releases;
            try
            {
                releases = await _client.Repository.Release.GetAll(rOwner, rName);
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

            try
            {
                var libdir = Path.Combine(_localFolder.Path, "Library", $"{model.Name}");
                if (!Directory.Exists(libdir))
                {
                    Directory.CreateDirectory(libdir);
                }
                var path = Path.Combine(libdir, filename);
               
                var uri = new Uri(url);
                var response = await client.GetAsync(uri);

                response.EnsureSuccessStatusCode();

                using var fs = new FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                await response.Content.CopyToAsync(fs);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            // udpate current info in model
            var version = latest.TagName;
            model.InstalledVersion = version;
            if (model.InstalledVersions is null)
            {
                model.InstalledVersions = new();
            }
            model.InstalledVersions.Add(version, filename);

            return true;

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
