using Abstractions.Models;

namespace Application.Common.Interfaces.Settings;

public interface ISettingsService
{
     Task LoadAsync(TypedSetting[] settingsMapping, CancellationToken cancellationToken = default);
     Task SetSetting<T>(TypedSetting<T> setting, T value, CancellationToken cancellationToken = default);
}