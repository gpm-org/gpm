using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using gpm.core.Exceptions;
using gpm.core.Extensions;
using gpm.core.Models;
using gpm.core.Util;
using gpm.core.Util.Builders;
using Octokit;
using Serilog;

namespace gpm.core.Services
{
    /// <summary>
    /// A service class for file deployments
    /// </summary>
    public class DeploymentService : IDeploymentService
    {
        private readonly ILibraryService _libraryService;
        private readonly IGitHubService _gitHubService;

        public DeploymentService(ILibraryService libraryService, IGitHubService gitHubService)
        {
            _libraryService = libraryService;
            _gitHubService = gitHubService;
        }

        /// <summary>
        /// Download and install an asset file from a Github repo.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="releases"></param>
        /// <param name="requestedVersion"></param>
        /// <param name="slot"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public async Task<bool> InstallReleaseAsync(
            Package package,
            IReadOnlyList<Release> releases,
            string? requestedVersion,
            int slot = 0)
        {
            using var ssc = new ScopedStopwatch();

            // check if any version is already installed, should never trigger
            if (_libraryService.TryGetValue(package.Id, out var model) && _libraryService.IsInstalledInSlot(package, slot))
            {
                var slotManifest = model.Slots[slot];
                var installedVersion = slotManifest.Version;
                if (installedVersion is not null /*&& installedVersion.Equals(version)*/)
                {
                    Log.Warning("[{Package}] Version {Version} already installed. Use gpm update or repair", package, installedVersion);
                    return false;
                }
            }

            // get correct release
            var release = string.IsNullOrEmpty(requestedVersion)
                ? releases[0] //latest
                : releases.FirstOrDefault(x => x.TagName.Equals(requestedVersion));
            if (release == null)
            {
                Log.Warning("No release found for version {RequestedVersion}", requestedVersion);
                return false;
            }

            // get correct release asset
            // TODO support multiple asset files?
            var assets = release.Assets;
            ArgumentNullException.ThrowIfNull(assets);
            var assetBuilder = IPackageBuilder.CreateDefaultBuilder<AssetBuilder>(package);
            var asset = assetBuilder.Build(release.Assets);
            if (asset is null)
            {
                Log.Warning("No release asset found for version {RequestedVersion}",
                    requestedVersion);
                return false;
            }

            // get download paths
            var version = release.TagName;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(version);
            if (!await _gitHubService.DownloadAssetToCache(package, asset, version))
            {
                Log.Warning("Failed to download package {Package}", package);
                return false;
            }

            // install asset
            var releaseFilename = asset.Name;
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(releaseFilename);
            if (!InstallPackageFromCache(package, version, slot))
            {
                Log.Warning("Failed to install package {Package}", package);
                return false;
            }


            // create or update package-lock
            // we use this everywhere (and not only for local packages) to support updaters
            // TODO: handle dependencies
            var destinationDir = _libraryService[package.Id].Slots[slot].FullPath.NotNullOrEmpty();
            var lockFilePath = Path.Combine(destinationDir, Constants.GPMLOCK);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            PackageLock lockfile = new();
            if (File.Exists(lockFilePath))
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<PackageLock>(await File.ReadAllTextAsync(lockFilePath), options);
                    if (obj is not null)
                    {
                        lockfile = obj;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to read existing lock file");
                }
            }

            if (!lockfile.Packages.Any(x => x.Id.Equals(package.Id)))
            {
                lockfile.Packages.Add(new PackageMeta(package.Id,
                    _libraryService[package.Id].Slots[slot].Version.NotNullOrEmpty()));
            }
            await File.WriteAllTextAsync(lockFilePath, JsonSerializer.Serialize(lockfile, options));

            return true;
        }

        /// <summary>
        /// Installs a package from the cache location by version and exact filename
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool InstallPackageFromCache(Package package, string version, int slot = 0)
        {
            using var ssc = new ScopedStopwatch();

            Log.Information("[{Package}] Installing from cache...", package);

            // checks
            var packageCacheFolder = Path.Combine(IAppSettings.GetCacheFolder(), $"{package.Id}", $"{version}");
            if (!Directory.Exists(packageCacheFolder))
            {
                throw new DirectoryNotFoundException();
            }
            if (!_libraryService.TryGetValue(package.Id, out var model))
            {
                // throw because the cache manifest needs to exist from a previous step
                throw new KeyNotFoundException();
            }
            if (!model.CacheData.TryGetValue(version, out var cacheManifest))
            {
                // throw because the cache manifest needs to exist from a previous step
                throw new KeyNotFoundException();
            }
            if (cacheManifest.Files is null)
            {
                Log.Warning("[{Package}] No files to install", package);
                return false;
            }
            if (cacheManifest.Files.Length < 1)
            {
                Log.Warning("[{Package}] No files to install", package);
                return false;
            }

            //TODO: support multiple files
            var assetCacheFile = cacheManifest.Files.First().Name;
            var assetCachePath = Path.Combine(packageCacheFolder, assetCacheFile);

            // get or create new slot
            var slotManifest = model.Slots.GetOrAdd(slot);

            // check if version is already installed
            var installedVersion = slotManifest.Version;
            if (installedVersion is not null && installedVersion.Equals(version))
            {
                Log.Information("[{Package}] Version {Version} already installed", package, version);
                return false;
            }

            // default install dirs
            slotManifest.FullPath ??= Directory.GetCurrentDirectory();
            var destinationDir = slotManifest.FullPath;

            // TODO: remove?
            // custom builder for install instructions
            var builder = IPackageBuilder.CreateDefaultBuilder<InstallBuilder>(package);
            destinationDir = builder.Build(destinationDir);

            // TODO ask or overwrite
            List<HashedFile>? installedFiles;
            if (package.ContentType is null)
            {
                var extension = Path.GetExtension(assetCachePath).ToLower();
                switch (extension)
                {
                    case ".zip":
                        installedFiles = ExtractZipArchiveTo(assetCachePath, destinationDir);
                        break;
                    case ".7z":
                    default:
                        // treat as single file
                        var releaseFilename = Path.GetFileName(assetCachePath);
                        var assetDestinationPath = Path.Combine(destinationDir, releaseFilename);
                        installedFiles = DeploySingleFile(assetCachePath, assetDestinationPath);
                        break;
                }
            }
            else
            {
                switch (package.ContentType)
                {
                    case EContentType.SingleFile:
                        var releaseFilename = Path.GetFileName(assetCachePath);
                        var assetDestinationPath = Path.Combine(destinationDir, releaseFilename);
                        installedFiles = DeploySingleFile(assetCachePath, assetDestinationPath);
                        break;
                    case EContentType.ZipArchive:
                        installedFiles = ExtractZipArchiveTo(assetCachePath, destinationDir);
                        break;
                    case EContentType.SevenZipArchive:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(package.ContentType), "Invalid Package content type.");
                }
            }

            if (installedFiles is null)
            {
                Log.Warning("[{Package}] No files installed. Aborting", package);
                return false;
            }

            // update library
            slotManifest.Version = version;
            slotManifest.Files = installedFiles;
            _libraryService.Save();

            return true;
        }

        /// <summary>
        /// Deploys a single file to its install destination
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destinationFileName"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        private static List<HashedFile> DeploySingleFile(string sourceFileName, string destinationFileName,
            bool overwrite = true)
        {
            // TODO: conflicts

            File.Copy(sourceFileName, destinationFileName, overwrite);

            Log.Information("Installed package to {DestinationFileName}", destinationFileName);

            return new List<HashedFile> { new HashedFile(destinationFileName, null, null) };
        }

        /// <summary>
        /// Extracts a zip archive to a given destination directory
        /// </summary>
        /// <param name="sourceArchiveFileName"></param>
        /// <param name="destinationDirectoryName"></param>
        /// <param name="overwriteFiles"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static List<HashedFile>? ExtractZipArchiveTo(string sourceArchiveFileName, string destinationDirectoryName,
            bool overwriteFiles = true)
        {
            var extension = Path.GetExtension(sourceArchiveFileName).ToLower();
            if (extension != ".zip")
            {
                throw new ArgumentException(null, nameof(sourceArchiveFileName));
            }

            // get the files in the zip archive
            var files = new List<string>();
            using (var archive = ZipFile.OpenRead(sourceArchiveFileName))
            {
                files.AddRange(from entry in archive.Entries
                               where !string.IsNullOrEmpty(entry.Name)
                               select entry.FullName);
            }

            // TODO: conflicts
            // check for conflicts with existing files
            //var conflicts = files.Where(x => File.Exists(Path.Combine(_settingsService.GetGameRootPath(), x)));
            //if (conflicts.Any())
            //{
            //    // ask user
            //    switch (await _interactionService.ShowConfirmation($"The following files will be overwritten, continue?\r\n\r\n {string.Join("\r\n", conflicts)}", "Install Mod"))
            //    {
            //        case WMessageBoxResult.None:
            //        case WMessageBoxResult.Cancel:
            //        case WMessageBoxResult.No:
            //            return null;
            //    }
            //}

            // extract to
            try
            {
                ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, overwriteFiles);
                foreach (var file in files)
                {
                    Log.Debug("Extracting file {File}", file);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Extracting to {DestinationDirectoryName} failed", destinationDirectoryName);
                return null;
            }

            Log.Information("Installed {SourceArchiveFileName} to {DestinationDirectoryName}",
                sourceArchiveFileName, destinationDirectoryName);

            return files
                .Select(x => new HashedFile(x, null, null))
                .ToList();
        }


        /// <summary>
        /// Uninstalls a package from the system by slot
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slotIdx"></param>
        /// <returns></returns>
        public async Task<bool> UninstallPackage(string key, int slotIdx = 0) =>
            _libraryService.TryGetValue(key, out var model) && await UninstallPackage(model, slotIdx);

        /// <summary>
        /// Uninstalls a package from the system by slot
        /// </summary>
        /// <param name="package"></param>
        /// <param name="slotIdx"></param>
        /// <returns></returns>
        public async Task<bool> UninstallPackage(Package package, int slotIdx = 0) =>
            _libraryService.TryGetValue(package.Id, out var model) && await UninstallPackage(model, slotIdx);

        /// <summary>
        /// Uninstalls a package from the system by slot
        /// </summary>
        /// <param name="model"></param>
        /// <param name="slotIdx"></param>
        public async Task<bool> UninstallPackage(PackageModel model, int slotIdx = 0)
        {
            if (!model.Slots.TryGetValue(slotIdx, out var slot))
            {
                Log.Warning("[{Package}] No package installed in slot {SlotIdx}", model,
                    slotIdx.ToString());
                return false;
            }

            Log.Information("[{Package}] Removing package from slot {SlotIdx}", model,
                slotIdx.ToString());

            var files = slot.Files
                .Select(x => x.Name)
                .ToList();
            var failed = new List<string>();

            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    Log.Warning("[{Package}] Could not find file {File} to delete. Skipping", model,
                        file);
                    continue;
                }

                try
                {
                    Log.Debug("[{Package}] Removing {File}", model, file);
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Log.Error(e, "[{Package}] Could not delete file {File}. Skipping", model, file);
                    failed.Add(file);
                }
            }

            // update package lock file
            var destinationDir = model.Slots[slotIdx].FullPath.NotNullOrEmpty();
            var lockFilePath = Path.Combine(destinationDir, Constants.GPMLOCK);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            PackageLock lockfile = new();
            if (File.Exists(lockFilePath))
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<PackageLock>(await File.ReadAllTextAsync(lockFilePath), options);
                    if (obj is not null)
                    {
                        lockfile = obj;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to read existing lock file");
                }
            }
            lockfile.Packages.RemoveAll(x => x.Id.Equals(model.Key));
            if (lockfile.Packages.Count > 0)
            {
                Directory.CreateDirectory(destinationDir);
                await File.WriteAllTextAsync(lockFilePath, JsonSerializer.Serialize(lockfile, options));
            }
            else
            {
                if (File.Exists(lockFilePath))
                {
                   FileS.TryDeleteFile(lockFilePath);
                }
            }

            // remove deploy manifest from library
            model.Slots.Remove(slotIdx);

            // TODO: remove cached files as well?
            _libraryService.Save();

            if (failed.Count != 0)
            {
                Log.Warning("[{Package}] Partially removed package. Could not delete:", model);
                foreach (var fail in failed)
                {
                    Log.Warning("Filename: {File}", fail);
                }

                return false;
            }

            Log.Information("[{Package}] Successfully removed package", model);
            return true;
        }
    }
}
