using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Handlers.Projections;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Entities;
using Analytics.Entities.Exceptions.Metrics;
using Analytics.Entities.Metrics;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Analytics;

namespace Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;

[Diagnostics(maxExecutionTimeMs: 200)]
[Transactional, AutoSave]
public record CreateCalculationJobCommand : ICommand<CreateCalculationJobResult>
{
    public string? MetricSystemName { get; }
    public MetricPayloadDto? MetricPayload { get; }
    public Guid UserId { get; }
    public Guid? MetricId { get; }

    public CreateCalculationJobCommand(
        string metricSystemName, 
        Guid userId, 
        MetricPayloadDto metricPayload)
    {
        MetricSystemName = metricSystemName;
        UserId = userId;
        MetricPayload = metricPayload;
    }

    public CreateCalculationJobCommand(Guid metricId, Guid userId)
    {
        MetricId = metricId;
        UserId = userId;
    }
}

public record CreateCalculationJobResult(MetricCalculationJob CalculationJob);

public class CreateCalculationJobHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope,
    IRepository<Metric, Guid> repository,
    IMetricCalculatorRegistry calculatorRegistry,
    IMetricConverterDispatcher dispatcher
    ) : ICommandHandler<CreateCalculationJobCommand, CreateCalculationJobResult>
{
    public async Task<CreateCalculationJobResult> Handle(
        CreateCalculationJobCommand request,
        CancellationToken cancellationToken)
    {
        MetricPayloadDto payload = null!;
        string systemName = null!;
        if (request.MetricId != null)
        {
            var metric = await repository.GetById(request.MetricId.Value, cancellationToken)
                ?? throw new MetricNotFoundException(request.MetricId.Value);
            payload = dispatcher.ToPayload(metric);
            systemName = calculatorRegistry.GetSystemName(metric.GetType());
        }
        
        var model = MetricCalculationJob.Create(request.MetricSystemName ?? systemName);

        if (request.MetricId != null)
            model.SetMetricId(request.MetricId.Value);
        
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        integrationEventScope.Add(new MetricCalculationRequestedEvent
        {
            RequestId = model.RequestId,
            CreatedBy = request.UserId,
            MetricSystemName = model.MetricSystemName,
            MetricPayload = MetricPayloadProjection.ToContract.AsFunc()(request.MetricPayload ?? payload)
        });
        
        return new CreateCalculationJobResult(model);
    }
}