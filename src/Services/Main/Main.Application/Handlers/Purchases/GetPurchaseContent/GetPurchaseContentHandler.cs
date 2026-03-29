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
    public async Task<GetPurchaseContentResult> Handle(
        GetPurchaseContentQuery request,
        CancellationToken cancellationToken)
    {
        var options = new QueryOptions<PurchaseContent, string>()
            {
                Data = request.Id
            }
            .WithTracking(false)
            .WithInclude(x => x.Article)
            .WithInclude(x => x.Article.Producer)
            .WithInclude(x => x.PurchaseContentLogistic);
        var content = await purchaseRepository
            .GetPurchaseContent(options, cancellationToken);
        return new GetPurchaseContentResult(content.Adapt<List<PurchaseContentDto>>());
    }
}