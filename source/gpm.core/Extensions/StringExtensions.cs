using System;
using System.Globalization;
using gpm.Core.Exceptions;

namespace gpm.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// https://github.com/octokit/octokit.net/blob/356588288e2d07fb4844911f6e03ef129540b124/Octokit/Helpers/StringExtensions.cs#L23
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Uri FormatUri(this string pattern, params object[] args)
        {
            ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

            return new Uri(string.Format(CultureInfo.InvariantCulture, pattern, args), UriKind.Relative);
        }
    }
}
