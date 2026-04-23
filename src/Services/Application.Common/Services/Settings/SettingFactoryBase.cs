using Application.Common.Interfaces.Settings;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Application.Common.Services.Settings;

public abstract class SettingFactoryBase : ISettingFactory
{
    protected readonly Dictionary<string, Func<string, Setting>> Map = new();

    public Setting Create(string key, string json)
    {
        return !Map.TryGetValue(key, out var creator) 
            ? throw new InvalidOperationException($"Unknown setting type: {key}") 
            : creator(json);
    }

    public Setting Create<T>(string json) where T : Setting, ISettingKey<T> =>Create(T.SettingName, json);

    public void Register<T>(Func<string, T> factory) where T : Setting, ISettingKey<T>
        => Map[T.SettingName] = factory;
}