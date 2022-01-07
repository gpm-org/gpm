using System;
using gpm.core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gpm.Tasks
{
    public partial class TaskService
    {
        /// <summary>
        /// Lists all installed packages
        /// TODO: make this only available globally, and don't save local packages to the library?
        /// </summary>
        /// <param name="host"></param>
        public void List()
        {
            Log.Information("Installed packages:");

            foreach (var (key, model) in _libraryService)
            {
                if (!_libraryService.IsInstalled(key))
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
