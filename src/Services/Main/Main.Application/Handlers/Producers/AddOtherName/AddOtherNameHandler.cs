using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Producer;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.Producers.AddOtherName;

[AutoSave]
[Transactional]
public record AddOtherNameCommand(int ProducerId, string OtherName, string WhereUsed) : ICommand<Unit>;

public class AddOtherNameHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<AddOtherNameCommand>
{
    public async Task<Unit> Handle(AddOtherNameCommand request, CancellationToken cancellationToken)
    {
        var model = ProducerOtherName.Create(request.ProducerId, request.OtherName, request.WhereUsed);
        await unitOfWork.AddAsync(model, cancellationToken);
        integrationEventScope.Add(new ProducerUpdatedEvent
        {
            Id = request.ProducerId
        });
        return Unit.Value;
    }
}