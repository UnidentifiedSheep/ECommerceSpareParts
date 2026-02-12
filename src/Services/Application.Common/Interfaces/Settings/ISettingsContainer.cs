using Abstractions.Models;

namespace Application.Common.Interfaces.Settings;

public interface ISettingsContainer
{
    bool Loaded { get; }
    T GetSetting<T>(TypedSetting<T> setting);
    bool TryGetValue(Type key, out object value);
    void SetSetting(Type key, object value);
}