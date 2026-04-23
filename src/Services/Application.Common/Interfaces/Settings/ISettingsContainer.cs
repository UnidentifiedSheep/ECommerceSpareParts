using Domain.CommonEntities;

namespace Application.Common.Interfaces.Settings;

public interface ISettingsContainer
{
    bool Loaded { get; }
    T Get<T>() where T : Setting;
    bool TryGet<T>(out T? value) where T : Setting;
    void Set<T>(T setting) where T : Setting;
}