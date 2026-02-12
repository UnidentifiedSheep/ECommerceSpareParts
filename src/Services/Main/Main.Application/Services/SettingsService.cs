using System.Text.Json;
using Abstractions.Models;
using Application.Common.Abstractions.Settings;
using Application.Common.Interfaces.Settings;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Handlers.Settings.SetSetting;
using MediatR;

namespace Main.Application.Services;

public class SettingsService(ISettingsRepository settingsRepository, IMediator mediator, ISettingsContainer settingsContainer) 
    : SettingsServiceBase(settingsContainer)
{
    private readonly ISettingsContainer _settingsContainer = settingsContainer;

    public override async Task LoadAsync(TypedSetting[] settingsMapping, CancellationToken cancellationToken = default)
    {
        var settings = (await settingsRepository.GetSettings(cancellationToken))
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var setting in settingsMapping)
        {
            if (!settings.TryGetValue(setting.Key, out var value))
            {
                await SetSetting(setting, setting.FallbackValue, cancellationToken);
                _settingsContainer.SetSetting(setting.Type, setting.FallbackValue);
                continue;
            }
            
            var typedValue = JsonSerializer.Deserialize(value, setting.Type) ??
                             throw new Exception("Не удалось десериализовать настройку.");
            
            _settingsContainer.SetSetting(setting.Type, typedValue);
        }
    }

    public override async Task SetSetting<T>(TypedSetting<T> setting, T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        var command = new SetSettingCommand(setting.Key, value);
        await mediator.Send(command, cancellationToken);
    }
    
    public async Task SetSetting(TypedSetting setting, object value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        var command = new SetSettingCommand(setting.Key, value);
        await mediator.Send(command, cancellationToken);
    }
}