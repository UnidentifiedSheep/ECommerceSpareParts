using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Main.Entities.Product;
using Main.Entities.Product.Stock;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductStockDeletionRule(IReadRepository<ProductStock, (string, int)> stocks)
	: IProductDeletionRule
{
	public int Order => 10;

	public async Task<ILocalizableMessage?> CheckAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> await stocks.Query.AnyAsync(
			stock => stock.ProductId == productId && stock.Quantity > 0,
			cancellationToken)
			? ArticleDeletionStockMessage.Instance
			: null;
}
