using Contracts.Products;
using Main.Application.Interfaces.Products;
using Main.Enums;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Application.Services.Products.EventParts;

public sealed class ProductCoreEventPartProvider : IProductEventPartProvider
{
	public ProductEventParts Part => ProductEventParts.Core;

	public bool IsHeavyRequest => false;

	public IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query) => query;

	public object Extract(IReadOnlyList<ProductEntity> products, DateTime updatedAt, IReadOnlyList<int> missingProductIds) =>
		new ProductsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = products
				.Select(product => new ProductSnapshot
				{
					Id = product.Id,
					Sku = product.Sku.Value,
					Name = product.Name.Value,
					ProducerId = product.ProducerId,
					ProductGroupId = product.ProductGroupId,
					Stock = product.Stock.Value,
					Indicator = product.Indicator?.Value
				})
				.Concat(missingProductIds
					.Select(id => new ProductSnapshot
					{
						Id = id,
						IsDeleted = true
					}))
				.ToArray()
		};
}
