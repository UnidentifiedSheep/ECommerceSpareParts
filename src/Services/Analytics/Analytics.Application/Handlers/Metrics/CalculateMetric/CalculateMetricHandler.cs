using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.Interfaces.Services;
using Analytics.Entities.Exceptions.Metrics;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces.Cqrs;

namespace Analytics.Application.Handlers.Metrics.CalculateMetric;

public record CalculateMetricCommand(Guid MetricId) : ICommand<CalculateMetricResult>;

public record CalculateMetricResult(Metric CalculatedMetric);

public class CalculateMetricHandler(
    IMetricCalculatorFactory calculatorFactory,
    IMetricRepository metricRepository)
    : ICommandHandler<CalculateMetricCommand, CalculateMetricResult>
{
    public async Task<CalculateMetricResult> Handle(CalculateMetricCommand request, CancellationToken cancellationToken)
    {
        var metric = await metricRepository.GetById(
            request.MetricId,
            cancellationToken) ?? throw new MetricNotFoundException(request.MetricId);

        var calculator = calculatorFactory.GetCalculator(metric.GetType());

        await calculator.CalculateMetric(metric, cancellationToken);

        return new CalculateMetricResult(metric);
    }
}
