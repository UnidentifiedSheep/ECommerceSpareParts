using Application.Common.Interfaces.Settings;
using Contracts.Settings;
using MassTransit;

namespace Main.Application.Consumers;

public class SettingChangedConsumer(ISettingsService settingsService) : IConsumer<SettingChangedEvent>
{
    public async Task Consume(ConsumeContext<SettingChangedEvent> context)
    {
        await settingsService.LoadAsync();
    }
}