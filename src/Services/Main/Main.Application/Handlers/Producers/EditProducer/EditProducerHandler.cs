using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Projections;
using Attributes;
using Contracts.Producer;
using Main.Application.Dtos.Producer;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;
using Main.Entities.Exceptions;

namespace Main.Application.Handlers.Producers.EditProducer;

[AutoSave]
[Transactional]
public record EditProducerCommand(int ProducerId, PatchProducerDto Producer) : ICommand<EditProducerResult>;

public record EditProducerResult(ProducerDto Producer);

public class EditProducerHandler(
    IProducerRepository repository,
    IProjectionProvider<Producer, ProducerDto> projection
    ) : ICommandHandler<EditProducerCommand, EditProducerResult>
{
    public async Task<EditProducerResult> Handle(
        EditProducerCommand request,
        CancellationToken cancellationToken)
    {
        var producer = await repository.GetById(request.ProducerId, cancellationToken)
                       ?? throw new ProducerNotFoundException(request.ProducerId);

        var patch = request.Producer;
        patch.Name.Apply(producer.SetName);
        patch.Description.Apply(producer.SetDescription);

        return new EditProducerResult(projection.Projection.AsFunc()(producer));
    }
}
