using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Handlers.Projections;
using Analytics.Entities;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Analytics;

namespace Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;

[AutoSave]
[Transactional]
public record CreateCalculationJobCommand(
    string MetricSystemName,
    MetricPayloadDto MetricPayload
    ) : ICommand<CreateCalculationJobResult>;

public record CreateCalculationJobResult(MetricCalculationJob CalculationJob);

public class CreateCalculationJobHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope,
    IUserContext userContext
) : ICommandHandler<CreateCalculationJobCommand, CreateCalculationJobResult>
{
    public async Task<CreateCalculationJobResult> Handle(
        CreateCalculationJobCommand request,
        CancellationToken cancellationToken)
    {
        var model = MetricCalculationJob.Create(request.MetricSystemName);
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        integrationEventScope.Add(new MetricCalculationRequestedEvent
        {
            RequestId = model.RequestId,
            CreatedBy = userContext.UserId,
            MetricSystemName = model.MetricSystemName,
            MetricPayload = MetricPayloadProjection.ToContract.AsFunc()(request.MetricPayload)
        });
        
        return new CreateCalculationJobResult(model);
    }
}