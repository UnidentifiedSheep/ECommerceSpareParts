using Main.Entities.Producer;
using Main.Entities.Product.Enrichment;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Interfaces;

namespace Tests.TestContexts.ProductEnrichment;

public class SupplierProductTestContext(
	DContext context,
	ProducerTestContext producerTestContext
	) : TestContextBase<DContext>(context), IDependentTestContext
{
	public IReadOnlyList<SupplierProduct> SupplierProducts { get; private set; } = null!;
	public IReadOnlyList<SupplierProductCross> Crosses { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		SupplierProducts = new SupplierProductBuilder(Faker)
			.WithNamesCount(2)
			.BuildMany(3)
			.Concat(new SupplierProductBuilder(Faker)
				.WithProducer(Faker.PickRandom<Producer>(producerTestContext.Producers).Name)
				.WithNamesCount(2)
				.BuildMany(2))
			.ToList();

		await DbContext.AddRangeAsync(SupplierProducts, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);

		Crosses =
		[
			new SupplierProductCrossBuilder(Faker)
				.WithSupplierProducts(SupplierProducts[0], SupplierProducts[1])
				.Build(),
			new SupplierProductCrossBuilder(Faker)
				.WithSupplierProducts(SupplierProducts[0], SupplierProducts[2])
				.Build(),
			new SupplierProductCrossBuilder(Faker)
				.WithSupplierProducts(SupplierProducts[1], SupplierProducts[2])
				.Build(),
			new SupplierProductCrossBuilder(Faker)
				.WithSupplierProducts(SupplierProducts[1], SupplierProducts[3])
				.Build()
		];
		await DbContext.AddRangeAsync(Crosses, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);
	}

	public static Type[] DependsOn => [typeof(ProducerTestContext)];
}
