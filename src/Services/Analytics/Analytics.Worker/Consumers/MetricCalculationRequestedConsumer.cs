using Analytics.Application.Handlers.Metrics.CreateMetric;
using Contracts.Analytics;
using MassTransit;
using MediatR;

namespace Analytics.Worker.Consumers;

public class MetricCalculationRequestedConsumer(IMediator mediator) : IConsumer<MetricCalculationRequestedEvent>
{
    public async Task Consume(ConsumeContext<MetricCalculationRequestedEvent> context)
    {
        var @event = context.Message;
        var creationResult = await mediator.Send(new CreateMetricCommand(
            @event.MetricSystemName, 
            @event.MetricPayload,
            @event.CreatedBy));
            //TODO: move orchestration to application layer.
        
    }
}