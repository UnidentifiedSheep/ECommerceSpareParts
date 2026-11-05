using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Products.Queries;

public record GetAvailableProductsStockQuery : IQuery<GetAvailableProductsStockResult>
{
	public GetAvailableProductsStockQuery(IEnumerable<GetAvailableProductsStockItem> items)
	{
		Items = items.Distinct().ToList();
	}

	public GetAvailableProductsStockQuery(GetAvailableProductsStockItem item) : this([item])
	{
	}

	public IReadOnlyList<GetAvailableProductsStockItem> Items { get; }
}

public record GetAvailableProductsStockItem(int ProductId, string StorageCode);

public record GetAvailableProductsStockResult(Dictionary<GetAvailableProductsStockItem, int> Stocks);

public class GetAvailableProductsStockHandler(IReadRepository<StorageContent, int> repository, TimeProvider timeProvider)
	: IQueryHandler<GetAvailableProductsStockQuery, GetAvailableProductsStockResult>
{
	public async Task<GetAvailableProductsStockResult> Handle(GetAvailableProductsStockQuery request, CancellationToken cancellationToken)
	{
		if (request.Items.Count == 0)
			return new GetAvailableProductsStockResult([]);

		var predicate = PredicateBuilder.New<StorageContent>(false);
		foreach (var item in request.Items)
			predicate = predicate.Or(content => content.ProductId == item.ProductId && content.StorageCode == item.StorageCode);

		var utcNow = timeProvider.GetUtcNow().UtcDateTime;
		var counts = await repository.Query
			.Where(predicate)
			.Where(content => content.Count > 0)
			.Select(content => new
			{
				content.ProductId,
				content.StorageCode,
				Available = Math.Max(0L, content.Count - content.Locks
					.Where(item => item.State == StorageContentLockState.Active &&
						(item.ExpiresAt == null || item.ExpiresAt > utcNow))
					.Sum(item => (long)item.Quantity))
			})
			.GroupBy(content => new { content.ProductId, content.StorageCode })
			.Select(group => new { group.Key.ProductId, group.Key.StorageCode, Quantity = group.Sum(content => content.Available) })
			.ToDictionaryAsync(
				item => new GetAvailableProductsStockItem(item.ProductId, item.StorageCode),
				item => checked((int)item.Quantity),
				cancellationToken);

		return new GetAvailableProductsStockResult(
			request.Items.ToDictionary(item => item, counts.GetValueOrDefault));
	}
}
