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
        /// <param name="requestedVersion"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        Task<bool> InstallReleaseAsync(Package package, string? requestedVersion = null, int slot = 0);
    }
}
