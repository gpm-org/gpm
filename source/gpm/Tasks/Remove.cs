using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Tasks
{
    public static class Remove
    {
        public static void Action(string name, int slot, bool all, IHost host)
        {
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            if (string.IsNullOrEmpty(name))
            {
                logger.Warning($"No package name specified to install.");
                return;
            }

            // what if a package is removed from the database?
            // deprecate instead?
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                logger.Warning($"package {name} not found in database");
                return;
            }
            if (!libraryService.TryGetValue(package.Id, out var model))
            {
                logger.Warning($"[{package.Id}] Package is not installed.");
                return;
            }
            if (!libraryService.IsInstalled(package))
            {
                logger.Warning($"[{package.Id}] Package is not installed.");
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
