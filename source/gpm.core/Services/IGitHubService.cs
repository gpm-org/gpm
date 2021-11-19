using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using gpm.core.Models;

namespace gpm.core.Services
{
    public interface IGitHubService
    {
        Task<bool> InstallLatestReleaseAsync(Package selectedSubreddit);
    }
}
