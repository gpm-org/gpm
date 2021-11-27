using System;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace gpm.Tasks
{
    public static class Installed
    {
        public static void Action(string pattern, string regex, IHost host)
        {
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var library = serviceProvider.GetRequiredService<ILibraryService>();

            logger.Information("Installed packages:");

            foreach (var (key, model) in library)
            {
                if (library.IsInstalled(key))
                {
                    Console.WriteLine($"{model.Key}");
                    foreach (var (slotIdx, manifest) in model.Slots)
                    {
                        // print installed slots
                        Console.WriteLine($"[Slot {slotIdx.ToString()}]\t{manifest.Version}\t{manifest.FullPath}");
                    }
                }
            }
        }

    }
}
