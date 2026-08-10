using Abstractions.Interfaces;
using Application.Common.DomainEventHandlers.Settings;
using Application.Common.Services.Events;
using Contracts.Settings;
using Domain.CommonEntities.Events;
using FluentAssertions;

namespace Tests.Tests.DomainEventHandlers.Settings;

public sealed class PublishSettingUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_SettingEvents_AddsRoutedIntegrationEvents()
    {
        var integrationEventScope = new IntegrationEventScope();
        var handler = new PublishSettingUpdatedEventHandler(
            new TestServiceDefinition(),
            integrationEventScope);
        var changedAt = DateTime.UtcNow;
        var batch = new Batch<SettingUpdatedDomainEvent>(
        [
            new SettingUpdatedDomainEvent(
                "first-setting",
                "{\"value\":1}",
                changedAt),
            new SettingUpdatedDomainEvent(
                "second-setting",
                "{\"value\":2}",
                changedAt.AddSeconds(1))
        ]);

        await handler.Handle(batch, CancellationToken.None);

        var envelopes = integrationEventScope.Flush();
        envelopes.Should().HaveCount(2);
        envelopes.Should().OnlyContain(x =>
            x.RoutingKey == TestServiceDefinition.Name);
        envelopes.Select(x => x.Message)
            .Should().BeEquivalentTo(
            [
                new SettingUpdatedEvent
                {
                    Key = "first-setting",
                    Value = "{\"value\":1}",
                    ChangedAt = changedAt
                },
                new SettingUpdatedEvent
                {
                    Key = "second-setting",
                    Value = "{\"value\":2}",
                    ChangedAt = changedAt.AddSeconds(1)
                }
            ]);
    }

    private sealed class TestServiceDefinition : IServiceDefinition
    {
        public const string Name = "test-service";
        public string ServiceName => Name;
    }
}
