using Abstractions.Models;
using Application.Common.Interfaces.Settings;

namespace Application.Common.Abstractions.Settings;

public abstract class SettingsServiceBase(ISettingsContainer settingsContainer) : ISettingsService
{
    public abstract Task LoadAsync(TypedSetting[] settingsMapping, CancellationToken cancellationToken = default);

    public abstract Task SetSetting<T>(TypedSetting<T> setting, T value, CancellationToken cancellationToken = default);
}