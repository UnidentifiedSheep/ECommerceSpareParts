using Abstractions.Interfaces.Services;
using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Entities;
using Analytics.Enums;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Analytics;
using Mapster;
using MassTransit;

using ContractMetricPayload = Contracts.Models.Metric.MetricPayloadDto;

namespace Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;

[AutoSave]
[Transactional]
public record CreateCalculationJobCommand(
    string MetricSystemName,
    MetricPayloadDto MetricPayload,
    Guid CreatedBy) : ICommand<CreateCalculationJobResult>;
public record CreateCalculationJobResult(MetricCalculationJob CalculationJob);

public class CreateCalculationJobHandler(
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint
    ) : ICommandHandler<CreateCalculationJobCommand, CreateCalculationJobResult>
{
    public async Task<CreateCalculationJobResult> Handle(CreateCalculationJobCommand request, CancellationToken cancellationToken)
    {
        var model = new MetricCalculationJob
        {
            RequestId = Guid.NewGuid(),
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow,
            Status = CalculationStatus.AwaitingWorker,
            MetricSystemName = request.MetricSystemName,
        };

        await publishEndpoint.Publish(new MetricCalculationRequestedEvent
        {
            RequestId = model.RequestId,
            CreatedBy = request.CreatedBy,
            MetricSystemName = model.MetricSystemName,
            MetricPayload = request.MetricPayload.Adapt<ContractMetricPayload>(),
        }, cancellationToken);
        
        await unitOfWork.AddAsync(model, cancellationToken);
        
        return new CreateCalculationJobResult(model);
    }
}