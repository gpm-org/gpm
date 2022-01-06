using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace gpm.Core.Models;

/// <summary>
/// https://github.com/octokit/octokit.net/blob/356588288e2d07fb4844911f6e03ef129540b124/Octokit/Models/Response/RepositoryTopics.cs
/// </summary>
public class RepositoryTopics
{
    public RepositoryTopics() { Names = new List<string>(); }

    public RepositoryTopics(IEnumerable<string> names)
    {
        var initialItems = names.ToList();
        Names = new ReadOnlyCollection<string>(initialItems);
    }

    public IReadOnlyList<string> Names { get; protected set; }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    internal string DebuggerDisplay => string.Format(CultureInfo.InvariantCulture,
                "RepositoryTopics: Names: {0}", string.Join(", ", Names));
}
