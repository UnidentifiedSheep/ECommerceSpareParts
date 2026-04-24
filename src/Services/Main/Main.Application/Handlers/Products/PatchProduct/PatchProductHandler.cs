using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions.Products;
using MediatR;

namespace Main.Application.Handlers.Products.PatchProduct;

[AutoSave]
[Transactional]
public record PatchArticleCommand(int ArticleId, PatchProductDto PatchProduct) : ICommand;

public class PatchProductHandler(
    IIntegrationEventScope integrationEventScope,
    IProductRepository productRepository)
    : ICommandHandler<PatchArticleCommand>
{
    public async Task<Unit> Handle(PatchArticleCommand request, CancellationToken cancellationToken)
    {
        var patch = request.PatchProduct;
        var product = await productRepository.GetById(request.ArticleId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ArticleId);

        if (patch.Description.IsSet) product.SetDescription(patch.Description);
        if (patch.CategoryId.IsSet) product.SetCategory(patch.CategoryId);
        
        if (patch.Sku is { IsSet: true, Value: not null }) 
            product.SetSku(patch.Sku.Value);

        if (patch.Name is { IsSet: true, Value: not null })
            product.SetName(patch.Name.Value);

        if (patch.ProducerId.IsSet) product.SetProducerId(patch.ProducerId);

        if (patch.Indicator.IsSet) product.SetIndicator(patch.Indicator.Value);

        if (patch.PackingUnit.IsSet) product.SetPackingUnit(patch.PackingUnit.Value);

        if (patch.PairId.IsSet) product.SetPair(patch.PairId.Value);

        integrationEventScope.Add(new ProductUpdatedEvent
        {
            Id = product.Id,
        });
        
        return Unit.Value;
    }
}