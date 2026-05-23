using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Producer;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Producers.DeleteProducer;

[AutoSave]
[Transactional]
public record DeleteProducerCommand(int Id) : ICommand;

public class DeleteProducerHandler(
    IProducerRepository repository, 
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<DeleteProducerCommand>
{
    public async Task<Unit> Handle(DeleteProducerCommand request, CancellationToken cancellationToken)
    {
        var producerId = request.Id;
        var producer = await repository.GetById(producerId, cancellationToken)
                       ?? throw new ProducerNotFoundException(producerId);

        await ValidateData(producerId, cancellationToken);

        unitOfWork.Remove(producer);
        integrationEventScope.Add(new ProducerUpdatedEvent
        {
            Id = request.Id
        });
        return Unit.Value;
    }

    private async Task ValidateData(int id, CancellationToken cancellationToken = default)
    {
        if (await repository.ProducerHasAnyArticle(id, cancellationToken))
            throw new CannotDeleteProducerWithArticlesException();
    }
}