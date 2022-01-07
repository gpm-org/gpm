// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace gpm.WinUI.ViewModels.Pages;

/// <summary>
/// A base class for viewmodels for sample pages in the app.
/// </summary>
public class PageViewModel : ObservableObject
{
    ///// <summary>
    ///// The <see cref="IFilesService"/> instance currently in use.
    ///// </summary>
    //private readonly IFilesService FilesServices = Ioc.Default.GetRequiredService<IFilesService>();

    public PageViewModel()
    {
        LoadDocsCommand = new AsyncRelayCommand<string>(LoadDocsAsync);
    }

    /// <summary>
    /// Gets the <see cref="IAsyncRelayCommand{T}"/> responsible for loading the source markdown docs.
    /// </summary>
    public IAsyncRelayCommand<string> LoadDocsCommand { get; }

    //private IReadOnlyDictionary<string, string>? texts;

    //public IReadOnlyDictionary<string, string>? Texts
    //{
    //    get => texts;
    //    set => SetProperty(ref texts, value);
    //}

    /// <summary>
    /// Implements the logic for <see cref="LoadDocsCommand"/>.
    /// </summary>
    /// <param name="name">The name of the docs file to load.</param>
    private /*async*/ Task LoadDocsAsync(string? name)
    {
        //if (name is null) return;

        //// Skip if the loading has already started
        //if (!(LoadDocsCommand.ExecutionTask is null)) return;

        //var path = Path.Combine("Assets", "docs", $"{name}.md");
        //using var stream = await FilesServices.OpenForReadAsync(path);
        //using var reader = new StreamReader(stream);
        //var text = await reader.ReadToEndAsync();

        //Texts = MarkdownHelper.GetParagraphs(text);

        //OnPropertyChanged(nameof(GetParagraph));

        return Task.CompletedTask;
    }
}

