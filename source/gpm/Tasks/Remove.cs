using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gpm.Tasks
{
    public static class Remove
    {
        public static void Action(string name, int slot, bool all, IHost host)
        {
            var serviceProvider = host.Services;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(Remove));
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();
            var dataBaseService = serviceProvider.GetRequiredService<IDataBaseService>();

            if (string.IsNullOrEmpty(name))
            {
                logger.LogWarning("No package name specified to install");
                return;
            }

            // what if a package is removed from the database?
            // deprecate instead?
            var package = dataBaseService.GetPackageFromName(name);
            if (package is null)
            {
                logger.LogWarning("package {Name} not found in database", name);
                return;
            }
            if (!libraryService.TryGetValue(package.Id, out var model))
            {
                logger.LogWarning("[{Package}] Package is not installed", package);
                return;
            }
            if (!libraryService.IsInstalled(package))
            {
                logger.LogWarning("[{Package}] Package is not installed", package);
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
