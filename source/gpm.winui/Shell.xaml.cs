using gpmWinui.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using gpmWinui.Views;
using System;
using System.Collections.Generic;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace gpmWinui
{
    public sealed partial class Shell : Window/*: UserControl*/
    {
        private readonly IReadOnlyCollection<NavigationItemModel> NavigationItems;

        public Shell()
        {
            this.InitializeComponent();

            NavigationItems = new[]
            {
                new NavigationItemModel(AppsItem, typeof(AppsPage)),
                new NavigationItemModel(PluginsItem, typeof(PluginsPage), "ObservableObject"),
            };
        }


        // Navigates to a sample page when a button is clicked
        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (NavigationItems.FirstOrDefault(item => item.Item == args.InvokedItemContainer)?.PageType is Type pageType)
            {
                NavigationFrame.Navigate(pageType);
            }
        }

        // Sets whether or not the back button is enabled
        private void NavigationFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            NavigationView.IsBackEnabled = ((Frame)sender).BackStackDepth > 0;
        }

        // Navigates back
        private void NavigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (NavigationFrame.BackStack.LastOrDefault() is PageStackEntry entry)
            {
                NavigationView.SelectedItem = NavigationItems.First(item => item.PageType == entry.SourcePageType).Item;

                NavigationFrame.GoBack();
            }
        }

        // Select the introduction item when the shell is loaded
        private void Shell_OnLoaded(object sender, RoutedEventArgs e)
        {
            NavigationView.SelectedItem = AppsItem;

            NavigationFrame.Navigate(typeof(AppsPage));
        }
    }
}
