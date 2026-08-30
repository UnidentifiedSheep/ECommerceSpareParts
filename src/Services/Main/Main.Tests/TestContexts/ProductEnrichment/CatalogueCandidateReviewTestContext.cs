using Enums;
using Main.Entities.Product;
using Main.Entities.Product.Enrichment;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Extensions;
using ProducerEntity = Main.Entities.Producer.Producer;

namespace Tests.TestContexts.ProductEnrichment;

public sealed class CatalogueCandidateReviewTestContext(DContext context) : TestContextBase<DContext>(context)
{
	public ProducerEntity Producer { get; private set; } = null!;

	public Product Product { get; private set; } = null!;

	public CatalogueCandidate Candidate { get; private set; } = null!;

	public SupplierProduct SupplierProduct { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		Producer = await new ProducerBuilder(Faker).WithName("Review producer").BuildAndAddToDb(DbContext);
		Product = await new ProductBuilder(Faker).WithProducerId(Producer.Id).BuildAndAddToDb(DbContext);
		Candidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("review-sku")
			.WithProducerId(Producer.Id)
			.WithProductId(Product.Id)
			.BuildAndAddToDb(DbContext);

		SupplierProduct = new SupplierProductBuilder(Faker)
			.WithSku("supplier-sku")
			.WithProducer("Supplier producer")
			.WithSupplier(Supplier.FavoritParts)
			.Build();
		Candidate.AddSupplierProduct(SupplierProduct);
		SupplierProduct.AddName("First supplier name");
		SupplierProduct.AddName("Second supplier name");

		await DbContext.AddAsync(SupplierProduct, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);
	}
}
