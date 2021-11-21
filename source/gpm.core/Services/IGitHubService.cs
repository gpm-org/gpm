using System.Threading.Tasks;
using gpm.core.Models;

namespace gpm.core.Services
{
    public interface IGitHubService
    {
        /// <summary>
        /// Installs a release from github
        /// </summary>
        /// <param name="package"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<bool> InstallReleaseAsync(Package package, string? version = null);
    }
}
