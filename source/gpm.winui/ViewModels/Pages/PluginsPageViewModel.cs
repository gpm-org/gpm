using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using gpm.core.Models;
using gpm.core.Services;

namespace gpmWinui.ViewModels.Pages;

public class PluginsPageViewModel : PageViewModel
{
    private readonly IDataBaseService _dataBaseService = Ioc.Default.GetRequiredService<IDataBaseService>();

    public PluginsPageViewModel()
    {
        ReloadTaskCommand = new RelayCommand(ReloadTask);

        Packages = new ObservableCollection<PluginViewModel>(_dataBaseService.Select(x => new PluginViewModel(x.Value)));

    }

    /// <summary>
    /// Gets the <see cref="ICommand"/> responsible for setting <see cref="MyTask"/>.
    /// </summary>
    public ICommand ReloadTaskCommand { get; }

    public ObservableCollection<PluginViewModel> Packages { get; }


    private PluginViewModel? _selectedPlugin;
    public PluginViewModel? SelectedPlugin
    {
        get => _selectedPlugin;
        set => SetProperty(ref _selectedPlugin, value);
    }














    private string? name;

    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private TaskNotifier? myTask;

    public Task? MyTask
    {
        get => myTask;
        private set => SetPropertyAndNotifyOnCompletion(ref myTask, value);
    }

    public void ReloadTask()
    {
        MyTask = Task.Delay(3000);
    }
}
