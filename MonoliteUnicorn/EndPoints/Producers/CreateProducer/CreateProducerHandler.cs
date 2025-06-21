using Core.Interface;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.CreateProducer;

public record CreateProducerCommand(IEnumerable<AmwNewProducerDto> NewProducers) : ICommand<Unit>;

public class CreateProducerHandler(DContext context) : ICommandHandler<CreateProducerCommand, Unit>
{
    public async Task<Unit> Handle(CreateProducerCommand request, CancellationToken cancellationToken)
    {
        await using var dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var newProducers = request.NewProducers.Adapt<List<Producer>>();
        await context.Producers.AddRangeAsync(newProducers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}