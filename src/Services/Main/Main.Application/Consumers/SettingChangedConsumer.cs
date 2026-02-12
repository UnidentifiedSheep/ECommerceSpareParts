using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Contracts.Settings;
using Main.Abstractions.Constants;
using MassTransit;

namespace Main.Application.Consumers;

public class SettingChangedConsumer(ISettingsService settingsService) : IConsumer<SettingChangedEvent>
{
    public async Task Consume(ConsumeContext<SettingChangedEvent> context)
    {
        if (Settings.AllSettings.All(x => x.Key != context.Message.Key)) return;
        await settingsService.LoadAsync(Settings.AllSettings);
    }
}