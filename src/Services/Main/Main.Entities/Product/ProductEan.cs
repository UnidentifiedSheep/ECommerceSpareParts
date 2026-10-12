using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Product;

public class ProductEan : Entity<ProductEan, ProductEanKey>, ILinqEntity<ProductEan, ProductEanKey>
{
	public int ProductId { get; private set; }

	public string Ean { get; private set; } = null!;
	public override ProductEanKey GetId() => new(ProductId, Ean);
	public static Expression<Func<ProductEan, ProductEanKey>> GetKeySelector()
		=> x => new ProductEanKey(x.ProductId, x.Ean);
	public static Expression<Func<ProductEan, bool>> GetEqualityExpression(ProductEanKey key)
		=> x => x.Ean == key.Ean && x.ProductId == key.ProductId;
}

public readonly record struct ProductEanKey(int ProductId, string Ean) : ICompositeKey
{

	public object[] ToArray() => [ProductId, Ean];
}
