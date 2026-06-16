using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Producer;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.Producers.DeleteOtherName;

[AutoSave]
[Transactional]
public record DeleteOtherNameCommand(int ProducerId, string OtherName) : ICommand;

public class DeleteOtherNameHandler(
    IRepository<ProducerOtherName, string> repository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<DeleteOtherNameCommand>
{
    public async Task<Unit> Handle(DeleteOtherNameCommand request, CancellationToken cancellationToken)
    {
        var producerOtherName = await repository.GetById(Producer.ToNormalizedName(request.OtherName), cancellationToken)
                                ?? throw new ProducersOtherNameNotFoundException(request.OtherName);

        if (producerOtherName.ProducerId != request.ProducerId)
            throw new ProducersOtherNameNotFoundException(request.OtherName);

        unitOfWork.Remove(producerOtherName);
        integrationEventScope.Add(new ProducerUpdatedEvent
        {
            Id = request.ProducerId
        });
        return Unit.Value;
    }
}
