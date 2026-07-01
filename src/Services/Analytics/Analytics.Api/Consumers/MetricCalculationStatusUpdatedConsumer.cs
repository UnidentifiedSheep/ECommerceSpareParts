using Analytics.Api.Hubs;
using Contracts.Analytics;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Analytics.Api.Consumers;

public class MetricCalculationStatusUpdatedConsumer(
    IHubContext<MetricCalculationHub> hubContext
) : IConsumer<MetricCalculationStatusUpdatedEvent>
{
    public async Task Consume(ConsumeContext<MetricCalculationStatusUpdatedEvent> context)
    {
        await hubContext.Clients.All.SendAsync("MetricCalculationStatusUpdated", context.Message);
    }
}