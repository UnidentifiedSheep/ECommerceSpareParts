using Abstractions.Interfaces.Services;
using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Application.Handlers.Projections;
using Analytics.Entities;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Analytics;

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
    IIntegrationEventScope integrationEventScope
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
            CreatedBy = request.CreatedBy,
            MetricSystemName = model.MetricSystemName,
            MetricPayload = MetricPayloadProjection.ToContract.AsFunc()(request.MetricPayload)
        });


        return new CreateCalculationJobResult(model);
    }
}
