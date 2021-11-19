using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using gpm.core.Models;
using Windows.Storage;

namespace gpmWinui.Services
{
    public class LibraryService : ObservableObject, ILibraryService
    {

        private Library? _library;

        private const string s_fileName = "library.json";

        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        public LibraryService()
        {

        }

        public async Task LoadAsync()
        {
            try
            {
                var sampleFile = await _localFolder.GetFileAsync(s_fileName);
                var jsonString = await FileIO.ReadTextAsync(sampleFile);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var obj = JsonSerializer.Deserialize<Library>(jsonString, options);
                if (obj == null)
                {
                    _library = new Library
                    {
                        Plugins = new()
                    };
                }
                else
                {
                    _library = obj;
                }
            }
            catch (FileNotFoundException)
            {
                // Cannot find file
                _library = new Library
                {
                    Plugins = new()
                };
            }
            catch (IOException e)
            {
                // Get information from the exception, then throw
                // the info to the parent method.
                if (e.Source != null)
                {
                    Debug.WriteLine("IOException source: {0}", e.Source);
                }
                throw;
            }

            // check self
            //foreach (var (key, value) in plugins)
            //{
            //    if (!Plugins.ContainsKey(key))
            //    {
            //        Plugins.Add(key, new PackageModel(key, value, 1));
            //    }
            //}

            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(_library, options);

            var sampleFile = await _localFolder.CreateFileAsync(s_fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, jsonString);
        }

        public Dictionary<string, PackageModel> Plugins => _library?.Plugins is null ? new() : _library.Plugins;
    }

}
