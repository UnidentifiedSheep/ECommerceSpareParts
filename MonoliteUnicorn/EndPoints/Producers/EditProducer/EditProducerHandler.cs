using Core.Interface;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.EditProducer;

public record EditProducerCommand(int ProducerId, PatchProducerDto EditProducer) : ICommand<Unit>;

public class EditProducerHandler(DContext context) : ICommandHandler<EditProducerCommand, Unit>
{
    public async Task<Unit> Handle(EditProducerCommand request, CancellationToken cancellationToken)
    {
        var producer = await context.Producers
            .FirstOrDefaultAsync(x => x.Id == request.ProducerId, cancellationToken) ?? throw new ProducerNotFoundException(request.ProducerId);
        request.EditProducer.Adapt(producer);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}