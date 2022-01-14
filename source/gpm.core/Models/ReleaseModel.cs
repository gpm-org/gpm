using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace gpm.Core.Models;

public class ReleaseModel
{
    public ReleaseModel(Release githubRelease)
    {
        TagName = githubRelease.TagName;
        Assets = githubRelease.Assets.Select(x => new ReleaseAssetModel(x));
    }

    public string TagName { get; internal set; }

    public IEnumerable<ReleaseAssetModel> Assets { get; private set; }
}

public class ReleaseAssetModel
{
    public ReleaseAssetModel(ReleaseAsset gitHubReleaseAsset)
    {
        Name = gitHubReleaseAsset.Name;
        BrowserDownloadUrl = gitHubReleaseAsset.BrowserDownloadUrl;
    }

    public string Name { get; internal set; }
    public string BrowserDownloadUrl { get; internal set; }
}
