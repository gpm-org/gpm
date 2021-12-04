using System;
using System.Threading.Tasks;
using gpm.core.Exceptions;
using gpm.core.Models;
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

            //TODO make this better?
            var removeAll = name.Equals("all");
            if (removeAll)
            {
                Log.Warning("You are about to remove all installed packages. To continue enter: y");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    return false;
                }
                if (!input.ToLower().Equals("y"))
                {
                    return false;
                }

                var r = true;
                var cnt = 0;
                foreach (var (_, value) in libraryService)
                {
                    if (libraryService.IsInstalledInSlot(value.Key, 0))
                    {
                        r = libraryService.UninstallPackage(value);
                        if (r)
                        {
                            cnt++;
                        }
                    }
                }

                if (cnt > 0)
                {
                    Log.Information("Removed {Cnt} installed packages", cnt);
                }
                else
                {
                    Log.Information("No installed packages found");
                }
                return await Task.FromResult(r);
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
