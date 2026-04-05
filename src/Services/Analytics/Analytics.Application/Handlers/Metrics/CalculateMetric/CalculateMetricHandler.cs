using Abstractions.Models.Repository;
using Analytics.Abstractions.Exceptions.Metrics;
using Analytics.Abstractions.Interfaces.Application;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces;

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
        var queryOptions = new QueryOptions<Metric, Guid>
        {
            Data = request.MetricId
        }.WithTracking();
        
        var metric = await metricRepository.GetMetric(
            queryOptions, 
            cancellationToken) ?? throw new MetricNotFoundException(request.MetricId);
        
        var calculator = calculatorFactory.GetCalculator(metric.GetType());
        
        await calculator.CalculateMetric(metric, cancellationToken);
        
        return new CalculateMetricResult(metric);
    }
}