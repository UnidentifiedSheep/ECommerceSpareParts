using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Producer;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Producers.EditProducer;

[AutoSave]
[Transactional]
public record EditProducerCommand(int ProducerId, PatchProducerDto Producer) : ICommand;

public class EditProducerHandler(IProducerRepository repository) : ICommandHandler<EditProducerCommand>
{
    public async Task<Unit> Handle(EditProducerCommand request, CancellationToken cancellationToken)
    {
        var producer = await repository.GetById(request.ProducerId, cancellationToken)
                       ?? throw new ProducerNotFoundException(request.ProducerId);

        var patch = request.Producer;

        if (patch.Name.IsSet && patch.Name.Value != null)
            producer.SetName(patch.Name.Value);

        if (patch.Description.IsSet)
            producer.SetDescription(patch.Description.Value);

        return Unit.Value;
    }
}