using Abstractions.Interfaces.Services;
using Analytics.Entities;
using Analytics.Enums;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Analytics;
using MassTransit;

namespace Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;

[Transactional]
public record CreateCalculationJobCommand(
    string MetricSystemName,
    string MetricPayload,
    Guid CreatedBy,
    CalculationStatus Status) : ICommand<CreateCalculationJobResult>;
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
            Status = request.Status,
            MetricSystemName = request.MetricSystemName,
        };

        await publishEndpoint.Publish(new MetricCalculationRequestedEvent
        {
            RequestId = model.RequestId,
            CreatedBy = request.CreatedBy,
            MetricSystemName = model.MetricSystemName,
            MetricPayload = request.MetricPayload,
        }, cancellationToken);
        
        await unitOfWork.AddAsync(model, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new CreateCalculationJobResult(model);
    }
}