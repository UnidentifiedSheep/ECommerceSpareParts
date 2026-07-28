using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Entities.DomainEvents.Product;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.Products.UpsertProductCrosses;

[AutoSave]
[Transactional]
public record UpsertProductCrossesCommand(
    IReadOnlyCollection<(int ProductId, int CrossProductId)> Crosses) : ICommand<Unit>;

public class UpsertProductCrossesHandler(
    IProductRepository productRepository,
    IDomainEventScope domainEventScope)
    : ICommandHandler<UpsertProductCrossesCommand, Unit>
{
    public async Task<Unit> Handle(
        UpsertProductCrossesCommand request,
        CancellationToken cancellationToken)
    {
        var crosses = request.Crosses
            .Select(x => ProductCross.Create(x.ProductId, x.CrossProductId))
            .DistinctBy(x => x.GetId())
            .ToList();

        if (crosses.Count == 0) return Unit.Value;

        await productRepository.UpsertProductCrosses(crosses, cancellationToken);

        domainEventScope.AddRange(
            crosses
                .SelectMany(x => new[] { x.LeftProductId, x.RightProductId })
                .Distinct()
                .Select(x => new ProductLinkageUpdatedDomainEvent(x)));

        return Unit.Value;
    }
}
