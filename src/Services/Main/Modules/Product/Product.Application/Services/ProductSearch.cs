using Application.Common.Interfaces.Repositories;
using Application.Common.Querying;
using Main.Entities.Product.Product.ValueObjects;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductSearch(IReadRepository<Entities.Product.Product.Product, int> products)
	: IProductSearch
{
	public IQueryable<int> MatchingIds(string term)
	{
		var trimmed = term.Trim();
		var pattern = LikePattern.Contains(trimmed);
		var normalizedSku = Sku.ToNormalized(trimmed);
		var skuPattern = LikePattern.Contains(normalizedSku);

		return products.Query
			.Where(product => EF.Functions.ILike(
					product.Name.Value,
					pattern,
					"\\") ||
				normalizedSku.Length > 0 && EF.Functions.ILike(
					product.Sku.NormalizedValue,
					skuPattern,
					"\\"))
			.Select(product => product.Id);
	}
}
