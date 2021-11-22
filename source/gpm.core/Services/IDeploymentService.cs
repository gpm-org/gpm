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
        /// <param name="releaseFilename"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void InstallPackageFromCache(Package package, string version, string releaseFilename);
    }
}
