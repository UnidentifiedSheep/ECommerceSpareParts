using Abstractions.Interfaces;
using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Settings;
using Domain.CommonEntities.Events;

namespace Application.Common.DomainEventHandlers.Settings;

public sealed class PublishSettingUpdatedEventHandler(
	IServiceDefinition serviceDefinition,
	IIntegrationEventScope integrationEventScope) : BatchableDomainEventHandler<SettingUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<SettingUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var events = notification.Items.Select(x => new SettingUpdatedEvent
		{
			Key = x.Key,
			Value = x.Value,
			ChangedAt = x.ChangedAt
		});

		integrationEventScope.AddRange(events, serviceDefinition.ServiceName);

		return Task.CompletedTask;
	}
}
