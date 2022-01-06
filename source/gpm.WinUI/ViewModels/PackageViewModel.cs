using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using gpm.Core.Models;
using gpm.Core.Services;
using gpm.Core.Tasks;

namespace gpmWinui.ViewModels
{
    public class PackageViewModel : ObservableObject
    {
        private Package _model;


        private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();
        private readonly ITaskService _taskService = Ioc.Default.GetRequiredService<ITaskService>();


        //public PackageViewModel(Package model)
        //{
        //    _model = model;

        //    InstallCommand = new AsyncRelayCommand(InstallAsync);
        //    CheckCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
        //    LaunchCommand = new AsyncRelayCommand(LaunchAsync);
        //}

        public PackageViewModel(Package model)
        {
            _model = model;

            InstallCommand = new AsyncRelayCommand(InstallAsync);
            CheckCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
        }

        public string? Version
        {
            get
            {
                // get default version unless a slot is selected
                if (IsInstalled && _libraryService.TryGetDefaultSlot(Id, out var slot))
                {
                    return slot.Version;
                }

                // TODO slot selection in UI

                return "-";
            }
        }

        public string Id => _model.Id;

        public string? Url => _model.Url;

        public bool IsInstalled => _libraryService.IsInstalled(_model.Id);

        public bool IsNotInstalled => !IsInstalled;


        public IAsyncRelayCommand InstallCommand { get; }

        public IAsyncRelayCommand CheckCommand { get; }


        private async Task CheckForUpdatesAsync()
        {
            await Task.Delay(1);
        }

        private async Task InstallAsync()
        {
            if (Id is not null)
            {
                await _taskService.UpdateAndInstall(Id, "", "", true);
            }
        }
    }
}
