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
using Nito.AsyncEx;

namespace RedCommunityToolkit.ViewModels
{
    public class PluginViewModel : ObservableObject
    {
        private PackageModel _model;

        /// <summary>
        /// Gets the <see cref="IGitHubService"/> instance to use.
        /// </summary>
        private readonly IGitHubService _gitHubService = Ioc.Default.GetRequiredService<IGitHubService>();
        
        /// <summary>
        /// An <see cref="AsyncLock"/> instance to avoid concurrent requests.
        /// </summary>
        private readonly AsyncLock _loadingLock = new();

        public PluginViewModel(PackageModel model)
        {
            _model = model;

            InstallCommand = new AsyncRelayCommand(InstallAsync);
            CheckCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
            LaunchCommand = new AsyncRelayCommand(LaunchAsync);
        }

        public string? Name => _model.Name;

        public string? Version => _model.InstalledVersion;

        public string? ID => _model.ID;

        public bool IsInstalled => !string.IsNullOrEmpty(_model.InstalledVersion);

        public bool IsNotInstalled => !IsInstalled;


        public IAsyncRelayCommand InstallCommand { get; }

        public IAsyncRelayCommand CheckCommand { get; }

        public IAsyncRelayCommand LaunchCommand { get; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task LaunchAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
           throw new NotImplementedException();
        }

        private async Task CheckForUpdatesAsync()
        {
            await Task.Delay(1);
        }

        private async Task InstallAsync()
        {
            using (await _loadingLock.LockAsync())
            {
                try
                {
                    await Task.Delay(1);
                    //var result = await _gitHubService.InstallLatestReleaseAsync(_model);
                    //if (result)
                    //{
                    //    OnPropertyChanged(nameof(IsInstalled));
                    //    OnPropertyChanged(nameof(IsNotInstalled));
                    //}
                }
                catch
                {
                    // Whoops!
                }
            }

        }
    }
}
