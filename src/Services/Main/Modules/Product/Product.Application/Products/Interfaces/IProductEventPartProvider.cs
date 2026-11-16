using Main.Entities.Product.Enums;
using ProductEntity = Main.Entities.Product.Product.Product;

namespace Main.Modules.Product.Application.Products.Interfaces;

public interface IProductEventPartProvider
{
	ProductEventParts Part { get; }

	bool IsHeavyRequest { get; }

	IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query);

	object Extract(
		IReadOnlyList<ProductEntity> products,
		DateTime updatedAt,
		IReadOnlyList<int> missingProductIds);
}
