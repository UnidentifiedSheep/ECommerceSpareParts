using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Exceptions;

namespace Main.Entities.Product;

public class ProductCross : Entity<ProductCross, (int, int)>, ILinqEntity<ProductCross, (int, int)>
{
	private ProductCross()
	{
	}

	private ProductCross(int left, int right)
	{
		if (left == right)
			throw new InvalidInputException("article.linkage.article.cannot.equal.cross.article");
		var min = Math.Min(left, right);
		var max = Math.Max(left, right);

		LeftProductId = min;
		RightProductId = max;
	}

	public int LeftProductId { get; }

	public int RightProductId { get; }

	public Product LeftProduct { get; private set; } = null!;

	public Product RightProduct { get; private set; } = null!;

	public static Expression<Func<ProductCross, (int, int)>> GetKeySelector() => x =>
		ValueTuple.Create(x.LeftProductId, x.RightProductId);

	public static Expression<Func<ProductCross, bool>> GetEqualityExpression((int, int) key) => x =>
		x.LeftProductId == key.Item1 && x.RightProductId == key.Item2;

	public static ProductCross Create(int id, int crossId) => new(id, crossId);

	public override (int, int) GetId() => (LeftProductId, RightProductId);
}
