using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using gpm.Core.Models;
using gpm.Core.Util;
using LibGit2Sharp;
using ProtoBuf;
using Serilog;

namespace gpm.Core.Services
{
    [ProtoContract]
    public class DataBaseServiceDto
    {
        public DataBaseServiceDto()
        {

        }
        public DataBaseServiceDto(int version, IEnumerable<Package> packages)
        {
            Packages = packages;
            Version = version;
        }

        [ProtoMember(1)]
        public int Version { get; set; }

        [ProtoMember(2)]
        public IEnumerable<Package>? Packages { get; set; }
    }


    public class DataBaseService : IDataBaseService
    {
        private int _loadAttempts = 5;

        private const int VERSION = 1;

        public DataBaseService()
        {
            Load();
        }

        private Dictionary<string, Package> _packages = new();

        #region serialization

        /// <summary>
        /// Deserialize from file
        /// </summary>
        public void Load()
        {
            if (File.Exists(IAppSettings.GetDbFile()))
            {
                try
                {
                    using var file = File.OpenRead(IAppSettings.GetDbFile());
                    var dto = Serializer.Deserialize<DataBaseServiceDto>(file);

                    ArgumentNullException.ThrowIfNull(dto);
                    ArgumentNullException.ThrowIfNull(dto.Packages);

                    _packages = dto.Packages.ToDictionary(x => x.Id);

                    if (dto.Version != VERSION)
                    {
                        throw new VersionNotFoundException();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to deserialize database");
                    if (File.Exists(IAppSettings.GetDbFile()))
                    {
                        File.Delete(IAppSettings.GetDbFile());
                    }
                    AttemptReload();
                }
            }
            else
            {
                AttemptReload();
            }
        }

        private void AttemptReload()
        {
            // delete local db and try again
            if (_loadAttempts <= 0)
            {
                throw new ProtoException();
            }

            Log.Information("No local database found, Attempting reload. Attempts left: {LoadAttempts}", _loadAttempts);
            _loadAttempts--;
            FetchAndUpdateSelf();
        }


        /// <summary>
        /// Serialize to file
        /// </summary>
        public void Save()
        {
            try
            {
                using var file = File.Create(IAppSettings.GetDbFile());
                Serializer.Serialize(file, new DataBaseServiceDto(VERSION, _packages.Values));
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
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    package = JsonSerializer.Deserialize<Package>(File.ReadAllText(file), options);
                }
                catch (Exception e)
                {
                    package = null;
                    Log.Error(e, "Deserialization of package {Package} failed", package);
                }

                if (package is not null)
                {
                    packages.Add(package);
                }
            }

            _packages = packages.ToDictionary(x => x.Id);

            try
            {
                Save();
            }
            catch (Exception)
            {
                if (File.Exists(IAppSettings.GetDbFile()))
                {
                    File.Delete(IAppSettings.GetDbFile());
                }
                throw;
            }

            Log.Information("Database updated");

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
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(status));
                }
            }
        }

        /// <summary>
        /// Fetch and Pull git database
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private static MergeStatus? UpdateGitDatabase()
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
            Log.Debug("Fetch log: {LogMessage}", logMessage);

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
                Log.Information("Status: {Status}", status);
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

        public bool ContainsName(string name) => _packages.Any(x => x.Value.Name.Equals(name));
        public IEnumerable<Package> LookupByName(string name) => ContainsName(name) ? _packages.Values.Where(x => x.Name.Equals(name)) : new List<Package>();

        public bool ContainsOwner(string name) => _packages.Any(x => x.Value.Owner.Equals(name));
        public IEnumerable<Package> LookupByOwner(string name) => ContainsOwner(name) ? _packages.Values.Where(x => x.Owner.Equals(name)) : new List<Package>();


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
                        Log.Warning("Multiple packages found in repository {Name}", name);
                        foreach (var item in packages)
                        {
                            Log.Information("Id: {ID}", item.Id);
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
                            Log.Warning("Multiple packages found in repository {Name}", name);
                            foreach (var item in packages)
                            {
                                Log.Information("Id: {ID}", item.Id);
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
                            Log.Warning("Multiple packages found in repository {Name}", name);
                            foreach (var item in packages)
                            {
                                Log.Information("Id: {ID}", item.Id);
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
