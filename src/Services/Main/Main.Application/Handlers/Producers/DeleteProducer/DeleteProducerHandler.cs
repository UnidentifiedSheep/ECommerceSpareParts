using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Producers;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Producers.DeleteProducer;

[Transactional]
public record DeleteProducerCommand(int Id) : ICommand;

public class DeleteProducerHandler(IProducerRepository producerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProducerCommand>
{
    public async Task<Unit> Handle(DeleteProducerCommand request, CancellationToken cancellationToken)
    {
        var producerId = request.Id;
        var producer = await producerRepository.GetProducer(producerId, true, cancellationToken)
                       ?? throw new ProducerNotFoundException(producerId);

        await ValidateData(producerId, cancellationToken);

        unitOfWork.Remove(producer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(int id, CancellationToken cancellationToken = default)
    {
        if (await producerRepository.ProducerHasAnyArticle(id, cancellationToken))
            throw new CannotDeleteProducerWithArticlesException();
    }
}