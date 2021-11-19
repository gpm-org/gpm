using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using gpm.core.Models;
using Refit;

namespace RedCommunityToolkit.Services
{
    public interface IGitHubService
    {
        Task<bool> InstallLatestReleaseAsync(PackageModel selectedSubreddit);
    }

    public sealed class PostsQueryResponse
    {
        /// <summary>
        /// Gets or sets the listing data for the response.
        /// </summary>
        //[JsonPropertyName("data")]
        //public PostListing? Data { get; set; }
    }
}
