using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace gpm.WinUI.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PackagesPage : Page
{
    public PackagesPage()
    {
        InitializeComponent();
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

