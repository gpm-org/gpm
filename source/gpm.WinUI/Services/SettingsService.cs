// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Foundation.Collections;
using Windows.Storage;

namespace gpm.WinUI.Services;

/// <summary>
/// A simple <see langword="class"/> that handles the local app settings.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    /// <summary>
    /// The <see cref="IPropertySet"/> with the settings targeted by the current instance.
    /// </summary>
    private readonly IPropertySet _settingsStorage = ApplicationData.Current.LocalSettings.Values;

    public string? Location
    {
        get => GetValue<string>(nameof(Location));
        set => SetValue(nameof(Location), value);
    }



    /// <inheritdoc/>
    private void SetValue<T>(string key, T? value)
    {
        if (!_settingsStorage.ContainsKey(key))
        {
            _settingsStorage.Add(key, value);
        }
        else
        {
            _settingsStorage[key] = value;
        }
    }

    /// <inheritdoc/>
    private T? GetValue<T>(string key)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (_settingsStorage.TryGetValue(key, out var value))
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        {
            return (T)value;
        }

        return default;
    }
}

