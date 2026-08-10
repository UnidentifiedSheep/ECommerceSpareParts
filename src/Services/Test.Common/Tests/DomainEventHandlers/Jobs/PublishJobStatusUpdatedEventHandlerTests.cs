using Abstractions.Interfaces;
using Application.Common.DomainEventHandlers.Jobs;
using Application.Common.Services.Events;
using Contracts.Job;
using Domain.CommonEntities.Job.Events;
using Domain.CommonEnums;
using FluentAssertions;

namespace Tests.Tests.DomainEventHandlers.Jobs;

public sealed class PublishJobStatusUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_StatusEvents_AddsRoutedIntegrationEvents()
    {
        var integrationEventScope = new IntegrationEventScope();
        var handler = new PublishJobStatusUpdatedEventHandler(
            new TestServiceDefinition(),
            integrationEventScope);
        var firstJobId = Guid.NewGuid();
        var secondJobId = Guid.NewGuid();
        var batch = new Batch<JobStatusUpdatedDomainEvent>(
        [
            new JobStatusUpdatedDomainEvent(
                firstJobId,
                JobStatus.Processing,
                2),
            new JobStatusUpdatedDomainEvent(
                secondJobId,
                JobStatus.Succeeded,
                1)
        ]);

        await handler.Handle(batch, CancellationToken.None);

        var envelopes = integrationEventScope.Flush();
        envelopes.Should().HaveCount(2);
        envelopes.Should().OnlyContain(x =>
            x.RoutingKey == TestServiceDefinition.Name);
        envelopes.Select(x => x.Message)
            .Should().BeEquivalentTo(
            [
                new JobStatusUpdatedEvent
                {
                    JobId = firstJobId,
                    Status = JobStatus.Processing.ToString(),
                    CurrentAttempt = 2
                },
                new JobStatusUpdatedEvent
                {
                    JobId = secondJobId,
                    Status = JobStatus.Succeeded.ToString(),
                    CurrentAttempt = 1
                }
            ]);
    }

    private sealed class TestServiceDefinition : IServiceDefinition
    {
        public const string Name = "test-service";
        public string ServiceName => Name;
    }
}
