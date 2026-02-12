using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Amw.Producers;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
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
        var newProducer = request.NewProducer.Adapt<Producer>();
        await unitOfWork.AddAsync(newProducer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProducerResult(newProducer.Id);
    }
}