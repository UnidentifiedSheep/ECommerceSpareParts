using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Application.Dtos.Producer;
using Main.Entities;
using Main.Entities.Producer;
using Mapster;

namespace Main.Application.Handlers.Producers.CreateProducer;

[Transactional]
public record CreateProducerCommand(NewProducerDto NewProducer) : ICommand<CreateProducerResult>;

public record CreateProducerResult(int ProducerId);

public class CreateProducerHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProducerCommand, CreateProducerResult>
{
    public async Task<CreateProducerResult> Handle(CreateProducerCommand request, CancellationToken cancellationToken)
    {
        var newProducer = request.NewProducer;
        var producer = Producer.Create(newProducer.Name, newProducer.Description);
        await unitOfWork.AddAsync(producer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProducerResult(producer.Id);
    }
}