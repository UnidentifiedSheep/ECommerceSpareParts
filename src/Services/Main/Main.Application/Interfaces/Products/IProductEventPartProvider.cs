using Main.Enums;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Application.Interfaces.Products;

public interface IProductEventPartProvider
{
	ProductEventParts Part { get; }

	bool IsHeavyRequest { get; }

	IQueryable<ProductEntity> Populate(IQueryable<ProductEntity> query);

	object Extract(IReadOnlyList<ProductEntity> products, DateTime updatedAt, IReadOnlyList<int> missingProductIds);
}
