using System.Collections.Concurrent;
using Abstractions.Models;
using Application.Common.Interfaces.Settings;

namespace Application.Common.Abstractions.Settings;

public class SettingsContainer : ISettingsContainer
{
    private readonly ConcurrentDictionary<Type, object> _settings = new();

    public bool Loaded { get; private set; }
    
    public T GetSetting<T>(TypedSetting<T> setting)
    {
        if (!Loaded) throw new InvalidOperationException("Настройки не инициализированы. Используйте ISettingsService LoadAsync().");
        if (TryGetValue(setting.Type, out var value)) return (T)value;
        throw new Exception($"Не удалось найти настройку {setting.Key}");
    }

    public bool TryGetValue(Type key, out object value)
    {
        var result = _settings.TryGetValue(key, out var temp);
        value = temp!;
        return result;
    }
    
    public void SetSetting(Type key, object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _settings.AddOrUpdate(key, value, (_, _) => value);
        Loaded = true;
    }
}