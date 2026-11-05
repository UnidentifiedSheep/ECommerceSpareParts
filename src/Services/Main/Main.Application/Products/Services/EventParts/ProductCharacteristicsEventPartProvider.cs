using Contracts.Products;
using Main.Application.Products.Interfaces;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Application.Products.Services.EventParts;

public sealed class ProductCharacteristicsEventPartProvider : IProductEventPartProvider
{
	public ProductEventParts Part => ProductEventParts.Characteristics;

	public bool IsHeavyRequest => false;

	public IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query) =>
		query.Include(product => product.Characteristics);

	public object Extract(IReadOnlyList<ProductEntity> products, DateTime updatedAt, IReadOnlyList<int> missingProductIds) =>
		new ProductCharacteristicsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = products
				.Select(product => new ProductCharacteristicsSnapshot(
					product.Id,
					product.Characteristics
						.OrderBy(characteristic => characteristic.CharacteristicId)
						.Select(characteristic => new ProductCharacteristicSnapshot(
							characteristic.CharacteristicId,
							characteristic.Value))
						.ToArray()))
				.ToArray()
		};
}
