using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using gpm.core.Models;
using gpm.core.Services;
using gpm.Tasks;
using Nito.AsyncEx;

namespace gpmWinui.ViewModels
{
    public class PluginViewModel : ObservableObject
    {
        private Package _model;

        private readonly IGitHubService _gitHubService = Ioc.Default.GetRequiredService<IGitHubService>();
        private readonly ILibraryService _libraryService = Ioc.Default.GetRequiredService<ILibraryService>();
        private readonly ITaskService _taskService = Ioc.Default.GetRequiredService<ITaskService>();

        /// <summary>
        /// An <see cref="AsyncLock"/> instance to avoid concurrent requests.
        /// </summary>
        private readonly AsyncLock _loadingLock = new();

        public PluginViewModel(Package model)
        {
            _model = model;

            InstallCommand = new AsyncRelayCommand(InstallAsync);
            CheckCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
            LaunchCommand = new AsyncRelayCommand(LaunchAsync);
        }

        //public string? Version => _model.InstalledVersion;

        public string? ID => _model.Id;

        public string? Url => _model.Url;

        public bool IsInstalled => _libraryService.IsInstalled(_model.Id);

        public bool IsNotInstalled => !IsInstalled;


        public IAsyncRelayCommand InstallCommand { get; }

        public IAsyncRelayCommand CheckCommand { get; }

        public IAsyncRelayCommand LaunchCommand { get; }

        private async Task LaunchAsync()
        {





            await Task.Delay(1);
        }

        private async Task CheckForUpdatesAsync()
        {
            await Task.Delay(1);
        }

        private async Task InstallAsync()
        {
            if (ID is not null)
            {
                await _taskService.UpdateAndInstall(ID, "", "", true);
            }
        }
    }
}
