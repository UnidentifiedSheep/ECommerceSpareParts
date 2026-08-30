using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Products;
using Main.Application.Models.Product;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public sealed record GetProductCrossesItem(int ProductId, Pagination Pagination, string[]? SortBy);

public record GetProductCrossesQuery(IReadOnlyCollection<GetProductCrossesItem> Items)
	: IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(
	IReadOnlyDictionary<GetProductCrossesItem, IReadOnlyList<ProductDto>> Crosses);

public class GetProductCrossesHandler(IProductProvider productProvider)
	: IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
	public async Task<GetProductCrossesResult> Handle(
		GetProductCrossesQuery request,
		CancellationToken cancellationToken)
	{
		var items = request.Items.Distinct().ToArray();
		var providerRequestsByItem = items.ToDictionary(
			item => item,
			item => new ProductCrossesRequestItem(item.ProductId, item.SortBy));

		var crossIds = await productProvider.GetProductsCrossesAsync(
			providerRequestsByItem.Values.ToArray(),
			cancellationToken);

		var paginatedIdsByItem = items.ToDictionary(
			item => item,
			item => crossIds[providerRequestsByItem[item]].ApplyPagination(item.Pagination).ToArray());

		var products = await productProvider.GetProductsOrSetAsync(
			paginatedIdsByItem.Values.SelectMany(x => x).Distinct(),
			cancellationToken);

		var crosses = paginatedIdsByItem.ToDictionary(
			item => item.Key,
			item => (IReadOnlyList<ProductDto>)item.Value.Select(productId => products[productId]).ToArray());

		return new GetProductCrossesResult(crosses);
	}
}
