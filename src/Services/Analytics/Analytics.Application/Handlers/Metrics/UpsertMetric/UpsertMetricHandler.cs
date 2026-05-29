using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces.Cqrs;
using Attributes;

namespace Analytics.Application.Handlers.Metrics.UpsertMetric;

[AutoSave]
public record UpsertMetricCommand(
    string MetricSystemName,
    MetricPayloadDto MetricPayload) : ICommand<UpsertMetricResult>;

public record UpsertMetricResult(Metric Metric);

public class UpsertMetricHandler(
    IMetricCalculatorRegistry calculatorRegistry,
    IMetricRepository metricRepository,
    IUnitOfWork unitOfWork,
    IMetricValidatorDispatcher validatorDispatcher,
    IMetricConverterDispatcher metricConverterDispatcher)
    : ICommandHandler<UpsertMetricCommand, UpsertMetricResult>
{
    public async Task<UpsertMetricResult> Handle(UpsertMetricCommand request, CancellationToken cancellationToken)
    {
        var metricType = calculatorRegistry.GetMetricType(request.MetricSystemName);
        var metric = metricConverterDispatcher.Convert(request.MetricPayload, metricType);

        await validatorDispatcher.ValidateAsync(metricType, metric, cancellationToken);

        var existingMetric = await metricRepository.GetByNaturalKeyAsync(
            metricType,
            metric.RangeStart,
            metric.RangeEnd,
            metric.DimensionHash,
            cancellationToken);
        if (existingMetric is not null)
            return new UpsertMetricResult(existingMetric);

        await unitOfWork.AddAsync(metric, cancellationToken);

        return new UpsertMetricResult(metric);
    }
}
