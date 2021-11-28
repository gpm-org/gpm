using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public static class Remove
    {
        public static void Action(string name, int slot, bool all, IHost host)
        {
            var serviceProvider = host.Services;
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            if (string.IsNullOrEmpty(name))
            {
                Log.Warning("No package name specified to install");
                return;
            }

            // what if a package is removed from the database?
            // deprecate instead?
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                Log.Warning("package {Name} not found in database", name);
                return;
            }
            if (!libraryService.TryGetValue(package.Id, out var model))
            {
                Log.Warning("[{Package}] Package is not installed", package);
                return;
            }
            if (!libraryService.IsInstalled(package))
            {
                Log.Warning("[{Package}] Package is not installed", package);
                return;
            }

            // check if all is set
            if (all)
            {
                foreach (var (slotIdx, _) in model.Slots)
                {
                    libraryService.UninstallPackage(package, slotIdx);
                }
            }
            else
            {
                libraryService.UninstallPackage(package, slot);
            }
        }
    }
}
