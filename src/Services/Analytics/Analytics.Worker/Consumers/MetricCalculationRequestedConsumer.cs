using Analytics.Application.Handlers.Metrics.CalculateFullMetric;
using Analytics.Application.Handlers.Projections;
using Application.Common.Extensions;
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
            MetricPayloadProjection.FromContract.AsFunc()(@event.MetricPayload)));
    }
}