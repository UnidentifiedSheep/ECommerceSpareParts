using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities;
using Main.Entities.Purchase;
using Mapster;

namespace Main.Application.Handlers.Purchases.GetPurchaseContent;

public record GetPurchaseContentQuery(string Id) : IQuery<GetPurchaseContentResult>;

public record GetPurchaseContentResult(List<PurchaseContentDto> Content);

public class GetPurchaseContentHandler(IPurchaseRepository purchaseRepository)
    : IQueryHandler<GetPurchaseContentQuery, GetPurchaseContentResult>
{
    public async Task<GetPurchaseContentResult> Handle(
        GetPurchaseContentQuery request,
        CancellationToken cancellationToken)
    {
        var options = new QueryOptions<PurchaseContent, string>()
            {
                Data = request.Id
            }
            .WithTracking(false)
            .WithInclude(x => x.Product)
            .WithInclude(x => x.Product.Producer)
            .WithInclude(x => x.PurchaseContentLogistic);
        var content = await purchaseRepository
            .GetPurchaseContent(options, cancellationToken);
        return new GetPurchaseContentResult(content.Adapt<List<PurchaseContentDto>>());
    }
}