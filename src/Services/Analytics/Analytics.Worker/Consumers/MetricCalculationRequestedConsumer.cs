using Analytics.Application.Handlers.Metrics.CalculateFullMetric;
using Contracts.Analytics;
using MassTransit;
using MediatR;

namespace Analytics.Worker.Consumers;

public class MetricCalculationRequestedConsumer(ISender sender) : IConsumer<MetricCalculationRequestedEvent>
{
    public async Task Consume(ConsumeContext<MetricCalculationRequestedEvent> context)
    {
        var @event = context.Message;
        
        await sender.Send(new CalculateFullMetricCommand(
            @event.RequestId,
            @event.MetricSystemName,
            @event.MetricPayload,
            @event.CreatedBy));
    }
}