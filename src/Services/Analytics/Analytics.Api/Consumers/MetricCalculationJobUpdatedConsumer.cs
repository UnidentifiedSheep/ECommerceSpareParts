using Analytics.Api.Hubs;
using Contracts.Metrics;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Analytics.Api.Consumers;

public class MetricCalculationJobUpdatedConsumer(
    IHubContext<MetricCalculationHub> hubContext
    ) : IConsumer<MetricCalculationJobUpdatedEvent>
{
    public async Task Consume(ConsumeContext<MetricCalculationJobUpdatedEvent> context)
    {
        await hubContext.Clients.All
            .SendAsync(
                "MetricCalculationJobUpdated",
                context.Message);
    }
}