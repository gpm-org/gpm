using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using gpm.core.Models;
using gpm.core.Util;
using LibGit2Sharp;
using ProtoBuf;

namespace gpm.core.Services
{
    public class DataBaseService : IDataBaseService
    {
        private readonly ILoggerService _loggerService;

        public DataBaseService(ILoggerService loggerService)
        {
            _loggerService = loggerService;

            Load();
        }

        public Dictionary<string, Package> Packages { get; set; } = new();

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
                _loggerService.Warning("No Database loaded, try gpm update.");
                return;
            }

            Packages = packages.ToDictionary(x => x.Id);
        }

        /// <summary>
        /// Serialize to file
        /// </summary>
        public void Save()
        {
            try
            {
                using var file = File.Create(IAppSettings.GetDbFile());
                Serializer.Serialize(file, Packages);
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
                    _loggerService.Error(e);
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

            _loggerService.Success("Database updated");

            Load();
        }

        /// <summary>
        /// Fetch and Pull git database and update
        /// </summary>
        public void FetchAndUpdateSelf()
        {
            using var sw = new ScopedStopwatch();

            // update database
            var status = GitDatabaseUtil.UpdateGitDatabase(_loggerService);

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

        #endregion

        #region dictionary helpers

        public bool Contains(string key) => Packages.ContainsKey(key);
        public Package? Lookup(string key) => Contains(key) ? Packages[key] : null;

        public bool ContainsUrl(string url) => Packages.Any(x => x.Value.Url.Equals(url));
        public IEnumerable<Package> LookupByUrl(string url) => ContainsUrl(url) ? Packages.Values.Where(x => x.Url.Equals(url)) : new List<Package>();

        public bool ContainsName(string name) => Packages.Any(x => x.Value.RepoName.Equals(name));
        public IEnumerable<Package> LookupByName(string name) => ContainsName(name) ? Packages.Values.Where(x => x.RepoName.Equals(name)) : new List<Package>();

        public bool ContainsOwner(string name) => Packages.Any(x => x.Value.RepoOwner.Equals(name));
        public IEnumerable<Package> LookupByOwner(string name) => ContainsOwner(name) ? Packages.Values.Where(x => x.RepoOwner.Equals(name)) : new List<Package>();


        public Package? GetPackageFromName(string name)
        {
            if (Path.GetExtension(name) == ".git")
            {
                var packages = LookupByUrl(name).ToList();
                if (packages.Count == 1)
                {
                    return packages.First();
                }
                else if (packages.Count > 1)
                {
                    // log results
                    foreach (var item in packages)
                    {
                        _loggerService.Warning($"Multiple packages found in repository {name}:");
                        _loggerService.Info(item.Id);
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
                    if (packages.Count == 1)
                    {
                        return packages.First();
                    }
                    else if (packages.Count > 1)
                    {
                        // log results
                        foreach (var item in packages)
                        {
                            _loggerService.Warning($"Multiple packages found for name {name}:");
                            _loggerService.Info(item.Id);
                        }
                    }
                }

                {
                    // try owner
                    var packages = LookupByOwner(name).ToList();
                    if (packages.Count == 1)
                    {
                        return packages.First();
                    }
                    else if (packages.Count > 1)
                    {
                        // log results
                        foreach (var item in packages)
                        {
                            _loggerService.Warning($"Multiple packages found for owner {name}:");
                            _loggerService.Info(item.Id);
                        }
                    }
                }
            }

            return null;
        }

        #endregion

    }
}
