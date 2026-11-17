using Contracts.Storage;
using Main.Entities.Product.Enums;
using Main.Modules.Product.Application.Products.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Main.Entities.Product.Product.Product;

namespace Main.Modules.Product.Application.Products.Services.EventParts;

public sealed class ProductStocksEventPartProvider : IProductEventPartProvider
{
	public ProductEventParts Part => ProductEventParts.Stocks;

	public bool IsHeavyRequest => true;

	public IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query)
		=> query.Include(product => product.Stocks);

	public object Extract(
		IReadOnlyList<ProductEntity> products,
		DateTime updatedAt,
		IReadOnlyList<int> missingProductIds)
		=> new ProductStocksSnapshotEvent
		{
			OccurredAt = updatedAt,
			Products = products.Select(product => new ProductStocksSnapshot
				{
					ProductId = product.Id,
					Stocks = product.Stocks
						.Where(stock => stock.Quantity > 0)
						.ToDictionary(stock => stock.StorageCode, stock => stock.Quantity)
				})
				.ToArray()
		};
}
