using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Models;
using Main.Entities.Storage;

namespace Main.Application.Handlers.StorageContents.GetStorageContentCosts;

public record GetStorageContentCostsQuery(IEnumerable<int> ProductIds, bool OnlyPositiveQty)
    : IQuery<GetStorageContentCostsResult>;

public record GetStorageContentCostsResult(IEnumerable<StorageContentPriceProjection> StorageContentCosts);

public class GetStorageContentCostsHandler(
    IReadRepository<StorageContent, int> repository)
    : IQueryHandler<GetStorageContentCostsQuery, GetStorageContentCostsResult>
{
    public async Task<GetStorageContentCostsResult> Handle(
        GetStorageContentCostsQuery request,
        CancellationToken cancellationToken)
    {
        string sql =
             """
             SELECT 
                 sc.id AS StorageContentId,
                 sc.product_id AS ArticleId,
                 sc.currency_id AS CurrencyId,
                 sc.buy_price AS Price,
                 pl.currency_id AS LogisticsCurrencyId,
                 pcl.price AS LogisticsPrice,
                 pc.id AS PurchaseContentId,
                 pc.count AS PurchaseContentCount,
                 sc.purchase_datetime AS PurchaseDatetime,
                 sc.count AS CurrentCount,
                 p.id AS PurchaseId
             FROM storage_content sc
             LEFT JOIN purchase_content pc ON sc.id = pc.storage_content_id
             LEFT JOIN purchase p ON pc.purchase_id = p.Id
             LEFT JOIN purchase_logistics pl ON p.id = pl.purchase_id
             LEFT JOIN purchase_content_logistics pcl ON pc.id = pcl.purchase_content_id
             WHERE sc.product_id = ANY(@productIds)
               AND (@onlyPositive OR sc.count > 0)
             """;
        
        var result = await repository.QuerySqlAsync<StorageContentPriceProjection>(
            sql: sql,
            param: new
            {
                onlyPositive = request.OnlyPositiveQty,
                productIds = request.ProductIds
            });
        return new GetStorageContentCostsResult(result);
    }
}