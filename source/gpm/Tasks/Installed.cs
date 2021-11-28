using System;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public static class Installed
    {
        public static void Action(string pattern, string regex, IHost host)
        {
            var serviceProvider = host.Services;
            var library = serviceProvider.GetRequiredService<ILibraryService>();

            Log.Information("Installed packages:");

            foreach (var (key, model) in library)
            {
                if (!library.IsInstalled(key))
                {
                    continue;
                }

                Console.WriteLine("{0}", model.Key);
                foreach (var (slotIdx, manifest) in model.Slots)
                {
                    // print installed slots
                    Console.WriteLine("[Slot {0}]\t{1}\t{2}", slotIdx.ToString(), manifest.Version, manifest.FullPath);
                }
            }
        }

    }
}
