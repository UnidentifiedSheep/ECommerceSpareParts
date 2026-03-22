using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.Purchases.GetPurchaseContent;

public record GetPurchaseContentQuery(string Id) : IQuery<GetPurchaseContentResult>;

public record GetPurchaseContentResult(List<PurchaseContentDto> Content);

public class GetPurchaseContentHandler(IPurchaseRepository purchaseRepository)
    : IQueryHandler<GetPurchaseContentQuery, GetPurchaseContentResult>
{
    public static readonly QueryOptions<PurchaseContent> Options = new QueryOptions<PurchaseContent>()
        .WithInclude(x => x.PurchaseContentLogistic);
    public async Task<GetPurchaseContentResult> Handle(GetPurchaseContentQuery request,
        CancellationToken cancellationToken)
    {
        var content = await purchaseRepository
            .GetPurchaseContentWithArticleData(request.Id, Options, cancellationToken);
        return new GetPurchaseContentResult(content.Adapt<List<PurchaseContentDto>>());
    }
}