using Application.Common.Interfaces.Settings;
using Contracts.Settings;
using MassTransit;

namespace Main.Modules.Platform.Application.Consumer;

public class SettingUpdatedConsumer(ISettingsService settingsService) : IConsumer<SettingUpdatedEvent>
{
	public async Task Consume(ConsumeContext<SettingUpdatedEvent> context)
		=> await settingsService.LoadAsync();
}
