using System.Linq.Expressions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Querying;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Products;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public sealed record GetProductCrossesQuery(
	int ProductId,
	Pagination Pagination,
	IReadOnlyCollection<string>? SortBy = null,
	string? SearchTerm = null,
	string? StockStorageCode = null) : IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(IReadOnlyList<ProductDto> Crosses);

public class GetProductCrossesHandler(
	IProductProvider productProvider,
	IReadRepository<Product, int> productsRepository)
	: IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
	public async Task<GetProductCrossesResult> Handle(
		GetProductCrossesQuery request,
		CancellationToken cancellationToken)
	{
		var crossIds = await productProvider.GetProductCrossesAsync(request.ProductId, cancellationToken);
		var query = productsRepository.Query.Where(product => crossIds.Contains(product.Id));
		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
		{
			var term = request.SearchTerm.Trim();
			var pattern = $"%{EscapePattern(term)}%";
			var normalizedSku = Sku.ToNormalized(term);
			var skuPattern = $"%{normalizedSku}%";
			query = query.Where(product =>
				EF.Functions.ILike(product.Name.Value, pattern, "\\") ||
				EF.Functions.ILike(product.Producer.Name, pattern, "\\") ||
				normalizedSku.Length > 0 && EF.Functions.ILike(product.Sku.NormalizedValue, skuPattern));
		}

		var ids = await SortCrosses(query, (request.SortBy ?? []).Append("id").ToArray(), request.StockStorageCode)
			.ApplyPagination(request.Pagination)
			.Select(product => product.Id)
			.ToArrayAsync(cancellationToken);
		var products = await productProvider.GetProductsOrSetAsync(ids, cancellationToken);
		return new GetProductCrossesResult(ids.Select(id => products[id]).ToArray());
	}

	private static IOrderedQueryable<Product> SortCrosses(
		IQueryable<Product> query, IReadOnlyCollection<string> sortBy, string? storageCode)
	{
		// Validate all sort keys through the standard mapper; constructing this query does not execute SQL.
		var defaultOrder = query.SortBy(sortBy);
		if (string.IsNullOrWhiteSpace(storageCode)) return defaultOrder;

		storageCode = storageCode.Trim();
		Expression<Func<Product, long>> stockCount = product => product.Stocks
			.Where(stock => stock.StorageCode == storageCode)
			.Sum(stock => (long)stock.Quantity);
		IOrderedQueryable<Product>? ordered = null;
		foreach (var sort in sortBy.Where(value => !string.IsNullOrWhiteSpace(value)))
		{
			var definition = QueryableSortBy.ParseToText(sort);
			if (definition.Field != "count")
			{
				ordered = ordered is null ? query.SortBy([sort]) : ordered.ThenSortBy([sort]);
				continue;
			}

			ordered = ordered is null
				? definition.Desc ? query.OrderByDescending(stockCount) : query.OrderBy(stockCount)
				: definition.Desc ? ordered.ThenByDescending(stockCount) : ordered.ThenBy(stockCount);
		}
		return ordered ?? defaultOrder;
	}

	private static string EscapePattern(string value) =>
		value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
