using Core.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.DeleteProducer;

public record DeleteProducerCommand(int Id) : ICommand<Unit>;

public class DeleteProducerHandler(DContext context) : ICommandHandler<DeleteProducerCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProducerCommand request, CancellationToken cancellationToken)
    {
        var producer = await context.Producers.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new  ProducerNotFoundException(request.Id);
        _ = await context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.ProducerId == request.Id, cancellationToken)
            ?? throw new CannotDeleteProducerWithArticlesException();
        context.Producers.Remove(producer);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}