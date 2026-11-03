using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Entities.DomainEvents.ProductStock;

namespace Main.Entities.Storage;

public class ProductStock : Entity<ProductStock, (string, int)>, ILinqEntity<ProductStock, (string, int)>
{
	private ProductStock() { }

	private ProductStock(string storageCode, int productId, int quantity)
	{
		if (string.IsNullOrWhiteSpace(storageCode) || storageCode.Trim().Length > 128)
			throw new InvalidOperationException("Storage code must contain between 1 and 128 characters.");
		if (productId <= 0)
			throw new InvalidOperationException("Product ID must be greater than zero.");

		StorageCode = storageCode.Trim();
		ProductId = productId;
		SetQuantity(quantity);
	}

	public string StorageCode { get; } = null!;
	public int ProductId { get; }
	public int Quantity { get; private set; }
	public Storage Storage { get; private set; } = null!;
	public Product.Product Product { get; private set; } = null!;

	public static ProductStock Create(string storageCode, int productId, int quantity = 0) =>
		new(storageCode, productId, quantity);

	public void SetQuantity(int quantity)
	{
		if (quantity < 0)
			throw new InvalidOperationException("Stock quantity must be greater than or equal to zero.");
		if (Quantity == quantity) return;
		Quantity = quantity;
		AddDomainEvent(new ProductStockUpdatedDomainEvent(StorageCode, ProductId));
	}

	public static Expression<Func<ProductStock, (string, int)>> GetKeySelector() =>
		stock => ValueTuple.Create(stock.StorageCode, stock.ProductId);

	public static Expression<Func<ProductStock, bool>> GetEqualityExpression((string, int) key) =>
		stock => stock.StorageCode == key.Item1 && stock.ProductId == key.Item2;

	public override (string, int) GetId() => (StorageCode, ProductId);
}
