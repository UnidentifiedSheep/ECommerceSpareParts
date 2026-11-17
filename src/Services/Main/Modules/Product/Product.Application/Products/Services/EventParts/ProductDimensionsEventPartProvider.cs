using Contracts.Products;
using Main.Entities.Product.Enums;
using Main.Modules.Product.Application.Products.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Main.Entities.Product.Product.Product;

namespace Main.Modules.Product.Application.Products.Services.EventParts;

public sealed class ProductDimensionsEventPartProvider : IProductEventPartProvider
{
	public ProductEventParts Part => ProductEventParts.Dimensions;

	public bool IsHeavyRequest => false;

	public IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query)
		=> query.Include(product => product.ProductSize);

	public object Extract(
		IReadOnlyList<ProductEntity> products,
		DateTime updatedAt,
		IReadOnlyList<int> missingProductIds)
		=> new ProductDimensionsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = products.Select(product =>
				{
					var size = product.ProductSize;
					return new ProductDimensionsSnapshot(
						product.Id,
						size is null
							? null
							: new ProductDimensionsData(
								size.Length,
								size.Width,
								size.Height,
								size.Unit));
				})
				.ToArray()
		};
}
