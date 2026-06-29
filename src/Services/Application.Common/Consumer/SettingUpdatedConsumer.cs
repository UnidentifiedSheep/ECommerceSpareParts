using Application.Common.Interfaces.Settings;
using Contracts.Settings;
using MassTransit;

namespace Application.Common.Consumer;

public class SettingUpdatedConsumer(ISettingsService settingsService) : IConsumer<SettingUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SettingUpdatedEvent> context)
    {
        await settingsService.LoadAsync();
    }
}