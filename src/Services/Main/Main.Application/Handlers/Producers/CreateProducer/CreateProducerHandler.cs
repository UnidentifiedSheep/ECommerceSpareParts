using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Producers;
using Main.Core.Dtos.Amw.Producers;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Producers.CreateProducer;

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