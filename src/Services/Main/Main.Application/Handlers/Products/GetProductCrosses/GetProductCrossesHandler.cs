using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Models;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Products;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public sealed record GetProductCrossesItem(int ProductId, Pagination Pagination, IReadOnlyCollection<string>? SortBy);

public record GetProductCrossesQuery(IReadOnlyCollection<GetProductCrossesItem> Items)
	: IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(
	IReadOnlyDictionary<GetProductCrossesItem, IReadOnlyList<ProductDto>> Crosses);

public class GetProductCrossesHandler(
	IProductProvider productProvider,
	IReadRepository<Product, int> productsRepository)
	: IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
	public async Task<GetProductCrossesResult> Handle(
		GetProductCrossesQuery request,
		CancellationToken cancellationToken)
	{
		var items = request.Items.Distinct().ToArray();
		var crossIds = await productProvider.GetProductsCrossesAsync(
			items.Select(item => item.ProductId), cancellationToken);
		var paginatedIdsByItem = new Dictionary<GetProductCrossesItem, int[]>();

		foreach (var item in items)
		{
			var ids = crossIds[item.ProductId];
			var sortBy = (item.SortBy ?? []).Append("id").ToArray();
			paginatedIdsByItem[item] = await productsRepository.Query
				.Where(product => ids.Contains(product.Id))
				.SortBy(sortBy)
				.ApplyPagination(item.Pagination)
				.Select(product => product.Id)
				.ToArrayAsync(cancellationToken);
		}

		var products = await productProvider.GetProductsOrSetAsync(
			paginatedIdsByItem.Values.SelectMany(x => x).Distinct(),
			cancellationToken);

		var crosses = paginatedIdsByItem.ToDictionary(
			item => item.Key,
			IReadOnlyList<ProductDto> (item) => item.Value.Select(productId => products[productId]).ToArray());

		return new GetProductCrossesResult(crosses);
	}
}
