using Contracts.Products;
using Main.Application.Interfaces.Products;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Application.Services.Products.EventParts;

public sealed class ProductDimensionsEventPartProvider : IProductEventPartProvider
{
	public ProductEventParts Part => ProductEventParts.Dimensions;

	public bool IsHeavyRequest => false;

	public IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query) =>
		query.Include(product => product.ProductSize);

	public object Extract(IReadOnlyList<ProductEntity> products, DateTime updatedAt, IReadOnlyList<int> missingProductIds) =>
		new ProductDimensionsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = products.Select(product =>
			{
				var size = product.ProductSize;
				return new ProductDimensionsSnapshot(
					product.Id,
					size is null ? null : new ProductDimensionsData(size.Length, size.Width, size.Height, size.Unit));
			}).ToArray()
		};
}
