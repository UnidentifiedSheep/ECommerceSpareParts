using Abstractions.Interfaces.Services;
using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Abstractions.Interfaces.Application;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces;
using Attributes;

namespace Analytics.Application.Handlers.Metrics.CreateMetric;

[AutoSave]
public record CreateMetricCommand(
    string MetricSystemName,
    MetricPayloadDto MetricPayload) : ICommand<CreateMetricResult>;

public record CreateMetricResult(Metric Metric);

public class CreateMetricHandler(
    IMetricCalculatorRegistry calculatorRegistry,
    IUnitOfWork unitOfWork,
    IMetricValidatorDispatcher validatorDispatcher,
    IMetricConverterDispatcher metricConverterDispatcher)
    : ICommandHandler<CreateMetricCommand, CreateMetricResult>
{
    public async Task<CreateMetricResult> Handle(CreateMetricCommand request, CancellationToken cancellationToken)
    {
        var metricType = calculatorRegistry.GetMetricType(request.MetricSystemName);
        var metric = metricConverterDispatcher.Convert(request.MetricPayload, metricType);

        await validatorDispatcher.ValidateAsync(metricType, metric, cancellationToken);

        await unitOfWork.AddAsync(metric, cancellationToken);

        return new CreateMetricResult(metric);
    }
}