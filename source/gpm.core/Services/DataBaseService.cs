using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using gpm.core.Models;
using gpm.core.Util;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace gpm.core.Services
{
    public class DataBaseService : IDataBaseService
    {
        private readonly ILogger<DataBaseService> _loggerService;

        public DataBaseService(ILogger<DataBaseService> loggerService)
        {
            _loggerService = loggerService;

            Load();
        }

        private Dictionary<string, Package> _packages = new();

        #region serialization

        /// <summary>
        /// Deserialize from file
        /// </summary>
        public void Load()
        {
            IEnumerable<Package>? packages = null;
            if (File.Exists(IAppSettings.GetDbFile()))
            {
                using var file = File.OpenRead(IAppSettings.GetDbFile());
                packages = Serializer.Deserialize<IEnumerable<Package>>(file);
            }

            if (packages is null)
            {
                _loggerService.LogWarning("No Database loaded, try gpm update");
                return;
            }

            _packages = packages.ToDictionary(x => x.Id);
        }

        /// <summary>
        /// Serialize to file
        /// </summary>
        public void Save()
        {
            try
            {
                using var file = File.Create(IAppSettings.GetDbFile());
                Serializer.Serialize(file, _packages);
            }
            catch (Exception)
            {
                if (File.Exists(IAppSettings.GetDbFile()))
                {
                    File.Delete(IAppSettings.GetDbFile());
                }
                throw;
            }
        }

        /// <summary>
        /// Re-create protobuf database from git database and reload
        /// </summary>
        public void UpdateSelf()
        {
            var files = Directory.GetFiles(IAppSettings.GetGitDbFolder(), "*.gpak", SearchOption.AllDirectories);
            var packages = new List<Package>();
            foreach (var file in files)
            {
                Package? package;
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    package = JsonSerializer.Deserialize<Package>(File.ReadAllText(file), options);
                }
                catch (Exception e)
                {
                    package = null;
                    _loggerService.LogError(e, "Deserialization of database failed");
                }

                if (package is not null)
                {
                    packages.Add(package);
                }
            }

            try
            {
                using var file = File.Create(IAppSettings.GetDbFile());
                Serializer.Serialize(file, packages);
            }
            catch (Exception)
            {
                if (File.Exists(IAppSettings.GetDbFile()))
                {
                    File.Delete(IAppSettings.GetDbFile());
                }
                throw;
            }

            _loggerService.LogDebug("Database updated");

            Load();
        }

        /// <summary>
        /// Fetch and Pull git database and update
        /// </summary>
        public void FetchAndUpdateSelf()
        {
            using var sw = new ScopedStopwatch();

            // update database
            var status = UpdateGitDatabase();

            // init database
            if (!File.Exists(IAppSettings.GetDbFile()))
            {
                UpdateSelf();
            }
            else
            {
                if (status.HasValue)
                {
                    switch (status.Value)
                    {
                        case MergeStatus.UpToDate:
                            // do nothing
                            break;
                        case MergeStatus.FastForward:
                            // re-create database
                            UpdateSelf();
                            break;
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(status));
                }
            }
        }

        /// <summary>
        /// Lists all packages in the database
        /// </summary>
        public void ListAllPackages()
        {
            _loggerService.LogInformation("Available packages:");
            Console.WriteLine("Id\tUrl");
            foreach (var (key, package) in this)
            {
                Console.WriteLine("{0}\t{1}", key, package.Url);
                //_loggerService.LogInformation("{Key}\t{Package}", key, package.Url);
            }
        }

        /// <summary>
        /// Fetch and Pull git database
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private MergeStatus? UpdateGitDatabase()
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
            _loggerService.LogTrace("Fetch log: {LogMessage}", logMessage);

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
                _loggerService.LogInformation("Status: {Status}", status);
            }
            else
            {
                throw new ArgumentNullException(nameof(result));
            }

            return status;
        }

        #endregion

        #region dictionary helpers

        public bool Contains(string key) => _packages.ContainsKey(key);
        public Package? Lookup(string key) => Contains(key) ? _packages[key] : null;

        public bool ContainsUrl(string url) => _packages.Any(x => x.Value.Url.Equals(url));
        public IEnumerable<Package> LookupByUrl(string url) => ContainsUrl(url) ? _packages.Values.Where(x => x.Url.Equals(url)) : new List<Package>();

        public bool ContainsName(string name) => _packages.Any(x => x.Value.RepoName.Equals(name));
        public IEnumerable<Package> LookupByName(string name) => ContainsName(name) ? _packages.Values.Where(x => x.RepoName.Equals(name)) : new List<Package>();

        public bool ContainsOwner(string name) => _packages.Any(x => x.Value.RepoOwner.Equals(name));
        public IEnumerable<Package> LookupByOwner(string name) => ContainsOwner(name) ? _packages.Values.Where(x => x.RepoOwner.Equals(name)) : new List<Package>();


        public Package? GetPackageFromName(string name)
        {
            if (Path.GetExtension(name) == ".git")
            {
                var packages = LookupByUrl(name).ToList();
                switch (packages.Count)
                {
                    case 1:
                        return packages.First();
                    case > 1:
                    {
                        _loggerService.LogWarning("Multiple packages found in repository {Name}", name);
                        foreach (var item in packages)
                        {
                            _loggerService.LogInformation("Id: {ID}", item.Id);
                        }

                        break;
                    }
                }
            }
            else if (name.Split('/').Length == 2)
            {
                var splits = name.Split('/');
                var id = $"{splits[0]}/{splits[1]}";
                return Lookup(id);
            }
            else if (name.Split('/').Length == 3)
            {
                var splits = name.Split('/');
                var id = $"{splits[0]}/{splits[1]}/{splits[2]}";
                return Lookup(id);
            }
            else
            {

                {
                    // try name
                    var packages = LookupByName(name).ToList();
                    switch (packages.Count)
                    {
                        case 1:
                            return packages.First();
                        case > 1:
                        {
                            _loggerService.LogWarning("Multiple packages found in repository {Name}", name);
                            foreach (var item in packages)
                            {
                                _loggerService.LogInformation("Id: {ID}", item.Id);
                            }

                            break;
                        }
                    }
                }

                {
                    // try owner
                    var packages = LookupByOwner(name).ToList();
                    switch (packages.Count)
                    {
                        case 1:
                            return packages.First();
                        case > 1:
                        {
                            _loggerService.LogWarning("Multiple packages found in repository {Name}", name);
                            foreach (var item in packages)
                            {
                                _loggerService.LogInformation("Id: {ID}", item.Id);
                            }

                            break;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region IDictionary

        public IEnumerator<KeyValuePair<string, Package>> GetEnumerator() => _packages.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_packages).GetEnumerator();

        public void Add(KeyValuePair<string, Package> item) => _packages.Add(item.Key, item.Value);

        public void Clear() => _packages.Clear();

        public bool Contains(KeyValuePair<string, Package> item) => _packages.Contains(item);

        public void CopyTo(KeyValuePair<string, Package>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<string, Package>>)_packages).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, Package> item) => _packages.Remove(item.Key);

        public int Count => _packages.Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, Package>>)_packages).IsReadOnly;

        public void Add(string key, Package value) => _packages.Add(key, value);

        public bool ContainsKey(string key) => _packages.ContainsKey(key);

        public bool Remove(string key) => _packages.Remove(key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Package value)
        {
            if (_packages.TryGetValue(key, out var innerValue))
            {
                value = innerValue;
                return true;
            }

            value = null;
            return false;
        }

        public Package this[string key]
        {
            get => _packages[key];
            set => _packages[key] = value;
        }

        public ICollection<string> Keys => _packages.Keys;

        public ICollection<Package> Values => _packages.Values;

        #endregion
    }
}
