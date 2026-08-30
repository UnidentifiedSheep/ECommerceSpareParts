using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Product.Enrichment;

public class SupplierProductCross : Entity<SupplierProductCross, SupplierProductCrossKey>,
	ILinqEntity<SupplierProductCross, SupplierProductCrossKey>
{

	private SupplierProductCross()
	{
	}

	private SupplierProductCross(int id, int crossId)
	{
		if (id == crossId)
			throw new InvalidOperationException("Id and cross cannot be the same");

		LeftId = Math.Min(id, crossId);
		RightId = Math.Max(id, crossId);
	}

	public int LeftId { get; init; }

	public int RightId { get; init; }

	public static Expression<Func<SupplierProductCross, SupplierProductCrossKey>> GetKeySelector() => x =>
		new SupplierProductCrossKey(x.LeftId, x.RightId);
	public static Expression<Func<SupplierProductCross, bool>> GetEqualityExpression(
		SupplierProductCrossKey key) => x => x.RightId == key.RightId && x.LeftId == key.LeftId;

	public static SupplierProductCross Create(int id, int crossId) => new(id, crossId);

	public override SupplierProductCrossKey GetId() => new(LeftId, RightId);
}

public readonly record struct SupplierProductCrossKey(int LeftId, int RightId) : ICompositeKey
{
	public object[] ToArray() => [LeftId, RightId];
}
