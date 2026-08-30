using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

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

public class GetAvailableProductsStockHandler(
	IReadRepository<StorageContent, int> storageContentReadRepository)
	: IQueryHandler<GetAvailableProductsStockQuery, GetAvailableProductsStockResult>
{
	public async Task<GetAvailableProductsStockResult> Handle(
		GetAvailableProductsStockQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Items.Count == 0)
			return new GetAvailableProductsStockResult([]);

		var requestedItems = request.Items.ToHashSet();

		var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
		var storageCodes = request.Items.Select(x => x.StorageCode).Distinct().ToList();

		var result = (await storageContentReadRepository
				.Query
				.Where(x => x.Count > 0)
				.Where(x => productIds.Contains(x.ProductId) && storageCodes.Contains(x.StorageCode))
				.GroupBy(x => new
				{
					x.ProductId, x.StorageCode
				})
				.Select(x => new
				{
					x.Key.ProductId,
					x.Key.StorageCode,
					AvailableStock = x.Sum(z => z.Count)
				})
				.ToListAsync(cancellationToken))
			.Where(x =>
				requestedItems.Contains(new GetAvailableProductsStockItem(x.ProductId, x.StorageCode)))
			.ToDictionary(
				x => new GetAvailableProductsStockItem(x.ProductId, x.StorageCode),
				x => x.AvailableStock);

		foreach (var item in request.Items)
			result.TryAdd(item, 0);

		return new GetAvailableProductsStockResult(result);
	}
}
