using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Dtos.Amw.Producers;
using Main.Abstractions.Exceptions.Producers;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Producer;
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
        
        PatchProducerDto patch = request.Producer;

        if (patch.Name.IsSet && patch.Name.Value != null) 
            producer.SetName(patch.Name.Value);

        if (patch.Description.IsSet)
            producer.SetDescription(patch.Description.Value);
        
        return Unit.Value;
    }
}