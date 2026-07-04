using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.Producer;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Producer;

namespace Main.Application.Handlers.Producers.CreateProducer;

[AutoSave]
[Transactional]
public record CreateProducerCommand(NewProducerDto NewProducer) : ICommand<CreateProducerResult>;

public record CreateProducerResult(ProducerDto Producer);

public class CreateProducerHandler(IUnitOfWork unitOfWork )
    : ICommandHandler<CreateProducerCommand, CreateProducerResult>
{
    public async Task<CreateProducerResult> Handle(
        CreateProducerCommand request,
        CancellationToken cancellationToken)
    {
        var newProducer = request.NewProducer;
        var producer = Producer.Create(newProducer.Name, newProducer.Description);
        await unitOfWork.AddAsync(producer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProducerResult(ProducerProjections.ToDto.AsFunc()(producer));
    }
}