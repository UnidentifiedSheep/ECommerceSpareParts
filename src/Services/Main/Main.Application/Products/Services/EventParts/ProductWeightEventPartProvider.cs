using Contracts.Products;
using Main.Application.Products.Interfaces;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Application.Products.Services.EventParts;

public sealed class ProductWeightEventPartProvider : IProductEventPartProvider
{
	public ProductEventParts Part => ProductEventParts.Weight;

	public bool IsHeavyRequest => false;

	public IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query) =>
		query.Include(product => product.ProductWeight);

	public object Extract(IReadOnlyList<ProductEntity> products, DateTime updatedAt, IReadOnlyList<int> missingProductIds) =>
		new ProductWeightUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = products.Select(product =>
			{
				var weight = product.ProductWeight;
				return new ProductWeightSnapshot(
					product.Id,
					weight is null ? null : new ProductWeightData(weight.Weight, weight.Unit));
			}).ToArray()
		};
}
