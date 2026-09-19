using Bogus;
using Main.Entities.Product.Enrichment;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public sealed class SupplierProductCrossBuilder(Faker faker) : BuilderBase<SupplierProductCross>(faker)
{
	public int? LeftSupplierProductId { get; private set; }

	public int? RightSupplierProductId { get; private set; }

	public SupplierProductCrossBuilder WithLeftSupplierProductId(int supplierProductId)
	{
		LeftSupplierProductId = supplierProductId;
		return this;
	}

	public SupplierProductCrossBuilder WithRightSupplierProductId(int supplierProductId)
	{
		RightSupplierProductId = supplierProductId;
		return this;
	}

	public SupplierProductCrossBuilder WithSupplierProducts(SupplierProduct left, SupplierProduct right)
	{
		return WithLeftSupplierProductId(left.Id).WithRightSupplierProductId(right.Id);
	}

	public override SupplierProductCross Build()
	{
		if (!LeftSupplierProductId.HasValue || !RightSupplierProductId.HasValue)
			throw new InvalidOperationException("Two supplier products are required to build a cross.");

		return SupplierProductCross.Create(LeftSupplierProductId.Value, RightSupplierProductId.Value);
	}
}
