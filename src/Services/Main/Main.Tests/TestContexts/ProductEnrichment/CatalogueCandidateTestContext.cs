using Main.Entities.Product.Enrichment;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Interfaces;

namespace Tests.TestContexts.ProductEnrichment;

public class CatalogueCandidateTestContext(
	DContext ctx,
	SupplierProductTestContext supplierProductTestContext,
	ProducerTestContext producerTestContext) : TestContextBase<DContext>(ctx), IDependentTestContext
{
	public IReadOnlyList<CatalogueCandidate> Candidates { get; private set; } = null!;

	public static Type[] DependsOn => [typeof(SupplierProductTestContext), typeof(ProducerTestContext)];

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var builders = new List<CatalogueCandidateBuilder>();

		foreach (var supplierProduct in supplierProductTestContext.SupplierProducts.Take(3))
		{
			var builder = new CatalogueCandidateBuilder(Faker);
			var producer =
				producerTestContext.Producers.FirstOrDefault(x => x.Name == supplierProduct.Producer) ??
				producerTestContext.Producers[0];

			builder
				.WithSku(supplierProduct.Sku.Value)
				.WithProducerId(producer.Id)
				.WithSupplierProduct(supplierProduct);

			builders.Add(builder);
		}

		Candidates = builders.Select(x => x.Build()).ToList();

		await DbContext.AddRangeAsync(Candidates, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);
	}
}
