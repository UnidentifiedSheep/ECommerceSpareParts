using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Producer;
using Main.Application.Dtos.Producer;
using Main.Application.Interfaces.Persistence;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Producers.EditProducer;

[AutoSave]
[Transactional]
public record EditProducerCommand(int ProducerId, PatchProducerDto Producer) : ICommand<EditProducerResult>;

public record EditProducerResult(ProducerDto Producer);

public class EditProducerHandler(
    IProducerRepository repository,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<EditProducerCommand, EditProducerResult>
{
    public async Task<EditProducerResult> Handle(EditProducerCommand request, CancellationToken cancellationToken)
    {
        var producer = await repository.GetById(request.ProducerId, cancellationToken)
                       ?? throw new ProducerNotFoundException(request.ProducerId);

        var patch = request.Producer;

        if (patch.Name is { IsSet: true, Value: not null })
            producer.SetName(patch.Name.Value);

        if (patch.Description.IsSet)
            producer.SetDescription(patch.Description.Value);
        
        integrationEventScope.Add(new ProducerUpdatedEvent
        {
            Id = producer.Id
        });

        return new EditProducerResult(ProducerProjections.ToDto.AsFunc()(producer));
    }
}