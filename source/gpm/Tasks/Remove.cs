using System.Threading.Tasks;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public static class Remove
    {
        public static async Task<bool> Action(string name, int slot, bool all, IHost host)
        {
            var serviceProvider = host.Services;
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            if (string.IsNullOrEmpty(name))
            {
                Log.Warning("No package name specified to install");
                return false;
            }

            // what if a package is removed from the database?
            // deprecate instead?
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                Log.Warning("package {Name} not found in database", name);
                return false;
            }
            if (!libraryService.TryGetValue(package.Id, out var model))
            {
                Log.Warning("[{Package}] Package is not installed", package);
                return false;
            }
            if (!libraryService.IsInstalled(package))
            {
                Log.Warning("[{Package}] Package is not installed", package);
                return false;
            }

            // check if all is set
            var result = true;
            if (all)
            {
                foreach (var (slotIdx, _) in model.Slots)
                {
                    result &= libraryService.UninstallPackage(package, slotIdx);
                }
            }
            else
            {
                result = libraryService.UninstallPackage(package, slot);
            }

            return await Task.FromResult(result);
        }
    }
}
