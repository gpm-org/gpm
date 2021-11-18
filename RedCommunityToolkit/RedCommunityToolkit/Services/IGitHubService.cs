using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RedCommunityToolkit.Models;
using Refit;

namespace RedCommunityToolkit.Services
{
    public interface IGitHubService
    {
        Task<PostsQueryResponse> GetGitHubRepoAsync(PluginModel selectedSubreddit);
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
