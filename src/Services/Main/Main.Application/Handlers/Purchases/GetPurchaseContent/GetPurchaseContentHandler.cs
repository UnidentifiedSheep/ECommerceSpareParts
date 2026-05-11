using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Amw.Purchase;

namespace Main.Application.Handlers.Purchases.GetPurchaseContent;

public record GetPurchaseContentQuery(string Id) : IQuery<GetPurchaseContentResult>;

public record GetPurchaseContentResult(List<PurchaseContentDto> Content);

public class GetPurchaseContentHandler : IQueryHandler<GetPurchaseContentQuery, GetPurchaseContentResult>
{
    public async Task<GetPurchaseContentResult> Handle(
        GetPurchaseContentQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}