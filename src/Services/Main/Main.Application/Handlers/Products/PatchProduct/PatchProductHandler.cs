using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.Products;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Products.PatchProduct;

[Transactional]
[AutoSave]
public record PatchProductCommand(int ProductId, PatchProductDto PatchProduct) : ICommand;

public class PatchProductHandler(
    IProductRepository productRepository
    ) : ICommandHandler<PatchProductCommand>
{
    public async Task<Unit> Handle(PatchProductCommand request, CancellationToken cancellationToken)
    {
        var patch = request.PatchProduct;
        var product = await productRepository.GetById(request.ProductId, cancellationToken)
                      ?? throw new ProductNotFoundException(request.ProductId);

        patch.Description.Apply(product.SetDescription);
        patch.CategoryId.Apply(product.SetCategory);
        patch.Sku.Apply(x => product.SetSku(x));
        patch.ProducerId.Apply(product.SetProducerId);
        patch.Indicator.Apply(x => product.SetIndicator(x));
        patch.PackingUnit.Apply(product.SetPackingUnit);
        patch.PairId.Apply(product.SetPair);
        patch.Name.Apply(x => product.SetName(x));

        return Unit.Value;
    }
}