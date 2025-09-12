using Application.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.Producers;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Producers;
using Mapster;

namespace Application.Handlers.Producers.CreateProducer;

[Transactional]
public record CreateProducerCommand(NewProducerDto NewProducer) : ICommand<CreateProducerResult>;

public record CreateProducerResult(int ProducerId);

public class CreateProducerHandler(IUnitOfWork unitOfWork, IProducerRepository producerRepository)
    : ICommandHandler<CreateProducerCommand, CreateProducerResult>
{
    public async Task<CreateProducerResult> Handle(CreateProducerCommand request, CancellationToken cancellationToken)
    {
        var producerName = request.NewProducer.ProducerName;

        await ValidateData(producerName, cancellationToken);

        var newProducer = request.NewProducer.Adapt<Producer>();
        await unitOfWork.AddAsync(newProducer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProducerResult(newProducer.Id);
    }

    private async Task ValidateData(string producerName, CancellationToken cancellationToken = default)
    {
        if (await producerRepository.IsProducerNameTaken(producerName, cancellationToken))
            throw new ProducerNameTakenException(producerName);
    }
}