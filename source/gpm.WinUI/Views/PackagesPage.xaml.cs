using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace gpmWinui.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PackagesPage : Page
    {
        public PackagesPage()
        {
            this.InitializeComponent();
        }

        private void OpenInNewTab(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(PackageTabView, "ShowTabs", true);
            PackageTabView.TabItems.Add(new TabViewItem()
            {
                Header = "Just an example"
            });
        }

        // Remove the requested tab from the TabView
        private void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
            if (PackageTabView.TabItems.Count == 1)
                VisualStateManager.GoToState(PackageTabView, "HideTabs", true);
        }
    }
}
