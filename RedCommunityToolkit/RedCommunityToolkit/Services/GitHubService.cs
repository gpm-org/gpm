using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using RedCommunityToolkit.Models;

namespace RedCommunityToolkit.Services
{
    public class GitHubService : IGitHubService
    {
        GitHubClient client = new GitHubClient(new ProductHeaderValue("RedCommunityToolkit"));

        public GitHubService()
        {
             
        }


        public async Task<PostsQueryResponse> GetGitHubRepoAsync(PluginModel selectedSubreddit)
        {


            var releases = await client.Repository.Release.GetAll("octokit", "octokit.net");
            var latest = releases[0];
            Console.WriteLine(
                "The latest release is tagged at {0} and is named {1}",
                latest.TagName,
                latest.Name);


            return new PostsQueryResponse();
        }
    }
}
