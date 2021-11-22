using System;
using System.Collections.Generic;
using System.IO;
using DynamicData.Kernel;
using gpm.core.Models;
using ProtoBuf;

namespace gpm.core.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly ILoggerService _loggerService;

        public LibraryService(ILoggerService loggerService)
        {
            _loggerService = loggerService;

            Load();
        }

        private Dictionary<string, PackageModel> Packages { get; set; } = new();

        public void Load()
        {
            if (File.Exists(IAppSettings.GetLocalDbFile()))
            {
                try
                {
                    using var file = File.OpenRead(IAppSettings.GetLocalDbFile());
                    Packages = Serializer.Deserialize<Dictionary<string, PackageModel>>(file);
                }
                catch (Exception)
                {
                    Packages = new Dictionary<string, PackageModel>();
                }

            }
            else
            {
                Packages = new Dictionary<string, PackageModel>();
            }

        }

        public void Save()
        {
            try
            {
                using var file = File.Create(IAppSettings.GetLocalDbFile());
                Serializer.Serialize(file, Packages);
            }
            catch (Exception)
            {
                if (File.Exists(IAppSettings.GetLocalDbFile()))
                {
                    File.Delete(IAppSettings.GetLocalDbFile());
                }
                throw;
            }
        }


        public bool Contains(string key) => Packages.ContainsKey(key);
        public Optional<PackageModel> Lookup(string key) => Contains(key)
            ? Optional<PackageModel>.ToOptional(Packages[key])
            : Optional<PackageModel>.None;

        public PackageModel GetOrAdd(Package package)
        {
            var key = package.Id;
            if (!Packages.ContainsKey(key))
            {
                Packages.Add(key, new PackageModel(key));
            }

            return Packages[key];
        }

        public void Remove(string id) => Packages.Remove(id);
        public IEnumerable<PackageModel> GetPackages() => Packages.Values;
    }
}
