using System.Collections.Concurrent;
using Application.Common.Interfaces.Settings;
using Domain.CommonEntities;

namespace Application.Common.Abstractions.Settings;

public class SettingsContainer : ISettingsContainer
{
    private readonly ConcurrentDictionary<Type, Setting> _settings = new();

    public bool Loaded { get; private set; }

    public T Get<T>() where T : Setting
    {
        if (!Loaded)
            throw new InvalidOperationException(
                "Настройки не инициализированы. Используйте ISettingsService.LoadAsync().");

        if (_settings.TryGetValue(typeof(T), out var value))
            return (T)value;

        throw new KeyNotFoundException($"Настройка {typeof(T).Name} не найдена");
    }

    public bool TryGet<T>(out T? value) where T : Setting
    {
        if (_settings.TryGetValue(typeof(T), out var temp))
        {
            value = (T)temp;
            return true;
        }

        value = null;
        return false;
    }

    public void Set<T>(T setting) where T : Setting
    {
        ArgumentNullException.ThrowIfNull(setting);
        _settings.AddOrUpdate(typeof(T), setting, (_, _) => setting);
        Loaded = true;
    }
}