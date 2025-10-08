using Core.Attributes;
using Core.Dtos.Amw.Producers;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Producers;
using Main.Application.Interfaces;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Producers.EditProducer;

[Transactional]
public record EditProducerCommand(int ProducerId, PatchProducerDto EditProducer) : ICommand;

public class EditProducerHandler(IProducerRepository producerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<EditProducerCommand>
{
    public async Task<Unit> Handle(EditProducerCommand request, CancellationToken cancellationToken)
    {
        var producer = await producerRepository.GetProducer(request.ProducerId, true, cancellationToken)
                       ?? throw new ProducerNotFoundException(request.ProducerId);
        request.EditProducer.Adapt(producer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}