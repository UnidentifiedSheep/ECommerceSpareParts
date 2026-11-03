using Application.Common.Interfaces.Cqrs;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models.Storage;

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

public class GetAvailableProductsStockHandler(IStorageContentRepository repository, TimeProvider timeProvider)
	: IQueryHandler<GetAvailableProductsStockQuery, GetAvailableProductsStockResult>
{
	public async Task<GetAvailableProductsStockResult> Handle(GetAvailableProductsStockQuery request, CancellationToken cancellationToken)
	{
		var counts = await repository.GetAvailableCountsAsync(
			request.Items.Select(item => new StorageContentSelection(item.ProductId, item.StorageCode)).ToArray(),
			timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
		return new GetAvailableProductsStockResult(request.Items.ToDictionary(item => item,
			item => counts.GetValueOrDefault((item.ProductId, item.StorageCode))));
	}
}
