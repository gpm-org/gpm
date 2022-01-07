using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace gpmWinui.Models
{
    /// <summary>
    /// A simple model for tracking sample pages associated with buttons.
    /// </summary>
    public sealed class NavigationItemModel
    {
        public NavigationItemModel(NavigationViewItem viewItem, Type pageType, string? name = null, string? tags = null)
        {
            Item = viewItem;
            PageType = pageType;
            Name = name;
            Tags = tags;
        }

        /// <summary>
        /// The navigation item for the current entry.
        /// </summary>
        public NavigationViewItem Item { get; }

        /// <summary>
        /// The associated page type for the current entry.
        /// </summary>
        public Type PageType { get; }

        /// <summary>
        /// Gets the name of the current entry.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the tag for the current entry, if any.
        /// </summary>
        public string? Tags { get; }
    }
}
