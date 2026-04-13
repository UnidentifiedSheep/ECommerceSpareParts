using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Producers;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.Producers.DeleteProducer;

[Transactional]
public record DeleteProducerCommand(int Id) : ICommand;

public class DeleteProducerHandler(IProducerRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProducerCommand>
{
    public async Task<Unit> Handle(DeleteProducerCommand request, CancellationToken cancellationToken)
    {
        var producerId = request.Id;
        var producer = await repository.GetById(producerId, cancellationToken)
                       ?? throw new ProducerNotFoundException(producerId);

        await ValidateData(producerId, cancellationToken);

        unitOfWork.Remove(producer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(int id, CancellationToken cancellationToken = default)
    {
        if (await repository.ProducerHasAnyArticle(id, cancellationToken))
            throw new CannotDeleteProducerWithArticlesException();
    }
}