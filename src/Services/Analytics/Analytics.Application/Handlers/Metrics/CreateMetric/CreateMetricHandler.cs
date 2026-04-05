using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Analytics.Abstractions.Interfaces.Application;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces;

namespace Analytics.Application.Handlers.Metrics.CreateMetric;

public record CreateMetricCommand(
    string MetricSystemName, 
    string MetricPayload, 
    Guid CreatedBy) : ICommand<CreateMetricResult>, IAutoSaveCommand;

public record CreateMetricResult(Metric Metric);

public class CreateMetricHandler(
    IMetricCalculatorRegistry calculatorRegistry,
    IUnitOfWork unitOfWork,
    IMetricValidatorDispatcher validatorDispatcher,
    IJsonSerializer serializer) 
    : ICommandHandler<CreateMetricCommand, CreateMetricResult>
{
    public async Task<CreateMetricResult> Handle(CreateMetricCommand request, CancellationToken cancellationToken)
    {
        Type metricType = calculatorRegistry.GetMetricType(request.MetricSystemName);
        
        Metric metric = (Metric?)serializer.Deserialize(request.MetricPayload, metricType) ??
                        throw new InvalidOperationException($"Metric {request.MetricPayload} could not be deserialized");

        metric.CreatedBy = request.CreatedBy;
        await validatorDispatcher.ValidateAsync(metricType, metric, cancellationToken);

        await unitOfWork.AddAsync(metric, cancellationToken);
        
        return new CreateMetricResult(metric);
    }
}