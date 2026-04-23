using Abstractions.Models;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Application.Common.Interfaces.Settings;

public interface ISettingsService
{
    Task LoadAsync(CancellationToken cancellationToken = default);
    Task SetSetting<T>(T value, CancellationToken cancellationToken = default) where T : Setting;
    Task<T> GetOrDefault<T>(CancellationToken cancellationToken = default) where T : Setting, ISetting<T>;
}