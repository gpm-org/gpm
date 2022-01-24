using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace gpm.Installer.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var _mainController = Ioc.Default.GetRequiredService<MainController>();

        var result = Nito.AsyncEx.AsyncContext.Run(() => _mainController.RunAsync());
        //var result = await _mainController.RunAsync();

        if (!result)
        {
            //Log.Warning("Helper installation failed.");
            Application.Current.Shutdown();
            return;
        }
    }
}
