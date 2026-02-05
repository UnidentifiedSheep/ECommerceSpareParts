using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Purchases.GetPurchaseContent;

public record GetPurchaseContentQuery(string Id) : IQuery<GetPurchaseContentResult>;

public record GetPurchaseContentResult(List<PurchaseContentDto> Content);

public class GetPurchaseContentHandler(IPurchaseRepository purchaseRepository)
    : IQueryHandler<GetPurchaseContentQuery, GetPurchaseContentResult>
{
    public async Task<GetPurchaseContentResult> Handle(GetPurchaseContentQuery request,
        CancellationToken cancellationToken)
    {
        var content = await purchaseRepository
            .GetPurchaseContent(request.Id, false, cancellationToken, x => x.PurchaseContentLogistic);
        return new GetPurchaseContentResult(content.Adapt<List<PurchaseContentDto>>());
    }
}