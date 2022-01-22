using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using gpm.Core.Installer;
using gpm.Core.Models;
using Serilog;

namespace gpm.Installer.WPF;

public class MainViewModel : ObservableRecipient
{
    private readonly MainController _mainController = Ioc.Default.GetRequiredService<MainController>();
    private readonly MySink _sink = Ioc.Default.GetRequiredService<MySink>();

    private readonly ReadOnlyObservableCollection<string> _list;

    public MainViewModel()
    {
        _text = "";

        var myOperation = _sink.Connect()
            .Transform(x => x.RenderMessage())
            .Bind(out _list)
            .DisposeMany()
            .Subscribe(x =>
            {
                foreach (var add in x)
                {
                    var msg = add.Item.Current;
                    Text += $"{msg}\n";
                }
            });

        Run();
    }

    private string _text;
    public string Text { get => _text; set => SetProperty(ref _text, value); }

    private void Run()
    {
        // checks
        // TODO


        // read lockfile
        if (!TryGetLockFile(out var lockFile))
        {
            Log.Error("No lockfile found in {BaseDir}", _mainController.BaseDir);
            Application.Current.Shutdown();
            return;
        }

        // wait for process to finish
        if (!WaitForProcessEnded())
        {
            Log.Error("Could not kill process, aborting");
            Application.Current.Shutdown();
            return;
        }

        // install in slot
        //TODO


        // restart app once finished
        if (_mainController.Restart)
        {
            Log.Information("Restarting ...");

            var exeName = _mainController.RestartName;
            Process.Start(exeName);
        }

        Log.Information("Shutting down ...");
        Application.Current.Shutdown();
    }

    private bool WaitForProcessEnded()
    {
        Thread.Sleep(2000);

        var pname = Path.GetFileNameWithoutExtension(_mainController.RestartName);
        var p = Process.GetProcessesByName(pname).FirstOrDefault();
        if (p == null)
        {
            Log.Information($"{pname} is null");
            Log.Information($"Waiting...");

            Thread.Sleep(2000);

            return true;
        }
        else
        {
            if (p.WaitForExit(5000))
            {
                Log.Information($"{pname} Has exited");
                return true;
            }
            else
            {
                Log.Error("Timeout");
                return false;
            }
        }
    }

    private bool TryGetLockFile([NotNullWhen(true)] out PackageLock? packageLock)
    {
        var baseDirectory = _mainController.BaseDir;

        Log.Information($"Using BaseDirectory: {baseDirectory}");

        var lockFilePath = Path.Combine(baseDirectory, gpm.Core.Constants.GPMLOCK);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (File.Exists(lockFilePath))
        {
            try
            {
                packageLock = JsonSerializer.Deserialize<PackageLock>(File.ReadAllText(lockFilePath), options);
                if (packageLock is null)
                {
                    return false;
                }
                var version = packageLock.Packages[0].Version;
                if (version is null)
                {
                    return false;
                }
                var id = packageLock.Packages[0].Id;
                if (id is null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to read existing lock file");
            }
        }
        packageLock = null;
        return false;
    }


}
