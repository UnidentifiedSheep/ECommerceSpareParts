using Abstractions.Interfaces;
using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Job;
using Domain.CommonEntities.Job.Events;

namespace Application.Common.DomainEventHandlers.Jobs;

public sealed class PublishJobStatusUpdatedEventHandler(
	IServiceDefinition serviceDefinition,
	IIntegrationEventScope integrationEventScope) : BatchableDomainEventHandler<JobStatusUpdatedDomainEvent>
{
	public override Task Handle(
		Batch<JobStatusUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var events = notification.Items.Select(x => new JobStatusUpdatedEvent
		{
			JobId = x.JobId,
			Status = x.Status.ToString(),
			CurrentAttempt = x.CurrentAttempt
		});

		integrationEventScope.AddRange(events, serviceDefinition.ServiceName);

		return Task.CompletedTask;
	}
}
