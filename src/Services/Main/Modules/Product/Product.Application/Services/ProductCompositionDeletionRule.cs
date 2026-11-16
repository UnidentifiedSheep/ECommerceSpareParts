using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Main.Entities.Product;
using Main.Entities.Product.Product;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductCompositionDeletionRule(IReadRepository<ProductContent, (int, int)> contents)
	: IProductDeletionRule
{
	public int Order => 60;

	public async Task<ILocalizableMessage?> CheckAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> await contents.Query.AnyAsync(content => content.ChildProductId == productId, cancellationToken)
			? ArticleDeletionCompositionMessage.Instance
			: null;
}
