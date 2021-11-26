using System;
using System.IO;
using gpm.core.Models;

namespace gpm.core.Services
{
    public interface IDeploymentService
    {
        /// <summary>
        /// Installs a package from the cache location by version and exact filename
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        bool InstallPackageFromCache(Package package, string version, int slot = 0);
    }
}
