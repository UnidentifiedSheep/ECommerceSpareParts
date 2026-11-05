using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Products.Queries;

public record GetProductsStockByIdsQuery : IQuery<GetProductsStockByIdsResult>
{
	public GetProductsStockByIdsQuery(IEnumerable<int> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public GetProductsStockByIdsQuery(int id) : this([id])
	{
	}

	public IReadOnlyList<int> Ids { get; }
}

public record GetProductsStockByIdsResult(Dictionary<int, long> Stocks);

public class GetProductsStockByIdsHandler(IReadRepository<ProductStock, (string, int)> repository)
	: IQueryHandler<GetProductsStockByIdsQuery, GetProductsStockByIdsResult>
{
	public async Task<GetProductsStockByIdsResult> Handle(
		GetProductsStockByIdsQuery request, CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0) return new GetProductsStockByIdsResult([]);

		var stocks = await repository.Query
			.Where(stock => request.Ids.Contains(stock.ProductId))
			.GroupBy(stock => stock.ProductId)
			.Select(group => new
			{
				ProductId = group.Key,
				Quantity = group.Sum(stock => (long)stock.Quantity)
			})
			.ToDictionaryAsync(
				stock => stock.ProductId,
				stock => stock.Quantity,
				cancellationToken);

		return new GetProductsStockByIdsResult(request.Ids.ToDictionary(id => id, stocks.GetValueOrDefault));
	}
}
