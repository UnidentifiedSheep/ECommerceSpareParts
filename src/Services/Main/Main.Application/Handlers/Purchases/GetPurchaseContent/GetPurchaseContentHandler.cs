using Application.Common.Interfaces;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities;
using Main.Entities.Purchase;
using Mapster;

namespace Main.Application.Handlers.Purchases.GetPurchaseContent;

public record GetPurchaseContentQuery(string Id) : IQuery<GetPurchaseContentResult>;

public record GetPurchaseContentResult(List<PurchaseContentDto> Content);

public class GetPurchaseContentHandler()
    : IQueryHandler<GetPurchaseContentQuery, GetPurchaseContentResult>
{
    public async Task<GetPurchaseContentResult> Handle(
        GetPurchaseContentQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}