using Application.Common.Interfaces;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Models;

namespace Main.Application.Handlers.StorageContents.GetStorageContentCosts;

public record GetStorageContentCostsQuery(IEnumerable<int> ArticleIds, bool OnlyPositiveQty) : IQuery<GetStorageContentCostsResult>;
public record GetStorageContentCostsResult(List<StorageContentPriceProjection> StorageContentCosts);

public class GetStorageContentCostsHandler(IStorageContentRepository storageContentRepository) 
    : IQueryHandler<GetStorageContentCostsQuery, GetStorageContentCostsResult>
{
    public async Task<GetStorageContentCostsResult> Handle(GetStorageContentCostsQuery request, CancellationToken cancellationToken)
    {
        var result = await storageContentRepository
            .GetStorageContentPricingInfo(request.ArticleIds, request.OnlyPositiveQty, cancellationToken);
        return new GetStorageContentCostsResult(result);
    }
}