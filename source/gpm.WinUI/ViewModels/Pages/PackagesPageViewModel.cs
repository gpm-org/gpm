using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using gpm.core.Models;
using gpm.core.Services;

namespace gpmWinui.ViewModels.Pages;

/// <summary>
/// View model for available packages
/// </summary>
public class PackagesPageViewModel : PageViewModel
{
    private readonly IDataBaseService _dataBaseService = Ioc.Default.GetRequiredService<IDataBaseService>();

    public PackagesPageViewModel()
    {
        ReloadTaskCommand = new RelayCommand(ReloadTask);

        Packages = new ObservableCollection<PackageViewModel>(_dataBaseService.Select(x => new PackageViewModel(x.Value)));

    }

    /// <summary>
    /// Gets the <see cref="ICommand"/> responsible for setting <see cref="MyTask"/>.
    /// </summary>
    public ICommand ReloadTaskCommand { get; }

    public ObservableCollection<PackageViewModel> Packages { get; }


    private PackageViewModel? _selectedPackage;
    public PackageViewModel? SelectedPackage    {
        get => _selectedPackage;
        set => SetProperty(ref _selectedPackage, value);
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
