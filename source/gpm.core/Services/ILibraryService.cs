using System.Diagnostics.CodeAnalysis;
using gpm.Core.Models;

namespace gpm.Core.Services
{
    public interface ILibraryService : IDictionary<string, PackageModel>
    {
        void Load();
        void Save();

        PackageModel GetOrAdd(Package package);

        bool IsInstalled(string key);
        bool IsInstalled(Package package);

        bool IsInstalledInSlot(Package package, int slot);

        /// <summary>
        /// Check if package is installed in slot
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        bool IsInstalledInSlot(string key, int slot);

        /// <summary>
        /// Get all installed packages
        /// </summary>
        /// <returns></returns>
        IEnumerable<PackageModel> GetInstalled() => Values.Where(x => IsInstalled(x.Key));

        bool IsInstalledAtLocation(Package package, string path, [NotNullWhen(true)] out int? idx);
        bool IsInstalledAtLocation(string key, string path, [NotNullWhen(true)] out int? idx);

        bool TryGetDefaultSlot(string key, [NotNullWhen(true)] out SlotManifest? slot);
        bool TryGetDefaultSlot(Package package, [NotNullWhen(true)] out SlotManifest? slot);
    }
}
