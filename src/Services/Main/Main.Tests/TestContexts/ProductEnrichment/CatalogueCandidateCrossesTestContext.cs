using Enums;
using Main.Entities.Product.Enrichment;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Extensions;
using ProducerEntity = Main.Entities.Producer.Producer;

namespace Tests.TestContexts.ProductEnrichment;

public sealed class CatalogueCandidateCrossesTestContext(DContext context) : TestContextBase<DContext>(context)
{
	public ProducerEntity Producer { get; private set; } = null!;

	public CatalogueCandidate Candidate { get; private set; } = null!;

	public CatalogueCandidate MappedCrossCandidate { get; private set; } = null!;

	public CatalogueCandidate CandidateWithoutCrosses { get; private set; } = null!;

	public CatalogueCandidate BatchCandidate { get; private set; } = null!;

	public SupplierProduct DirectSource { get; private set; } = null!;

	public SupplierProduct ReverseSource { get; private set; } = null!;

	public SupplierProduct MappedCross { get; private set; } = null!;

	public SupplierProduct NotMappedCross { get; private set; } = null!;

	public SupplierProduct BatchNotMappedCross { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		Producer = await new ProducerBuilder(Faker)
			.WithName("Cross producer")
			.BuildAndAddToDb(DbContext);

		Candidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("source-candidate")
			.WithProducerId(Producer.Id)
			.BuildAndAddToDb(DbContext);
		MappedCrossCandidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("mapped-cross-candidate")
			.WithProducerId(Producer.Id)
			.BuildAndAddToDb(DbContext);
		CandidateWithoutCrosses = await new CatalogueCandidateBuilder(Faker)
			.WithSku("candidate-without-crosses")
			.WithProducerId(Producer.Id)
			.BuildAndAddToDb(DbContext);
		BatchCandidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("batch-candidate")
			.WithProducerId(Producer.Id)
			.BuildAndAddToDb(DbContext);

		DirectSource = await AddSupplierProduct(
			Candidate,
			"direct-source",
			"Direct source producer",
			"Direct source name");
		MappedCross = await AddSupplierProduct(
			MappedCrossCandidate,
			"mapped-cross",
			"Mapped cross producer",
			"Mapped cross name");
		NotMappedCross = await AddSupplierProduct(
			null,
			"not-mapped-cross",
			"Not mapped cross producer",
			"Not mapped cross name");
		ReverseSource = await AddSupplierProduct(
			Candidate,
			"reverse-source",
			"Reverse source producer",
			"Reverse source name");
		var batchSource = await AddSupplierProduct(
			BatchCandidate,
			"batch-source",
			"Batch source producer",
			"Batch source name");
		BatchNotMappedCross = await AddSupplierProduct(
			null,
			"batch-not-mapped-cross",
			"Batch not mapped cross producer",
			"Batch not mapped cross name");

		await new SupplierProductCrossBuilder(Faker)
			.WithSupplierProducts(DirectSource, MappedCross)
			.BuildAndAddToDb(DbContext);
		await new SupplierProductCrossBuilder(Faker)
			.WithSupplierProducts(ReverseSource, NotMappedCross)
			.BuildAndAddToDb(DbContext);
		await new SupplierProductCrossBuilder(Faker)
			.WithSupplierProducts(batchSource, BatchNotMappedCross)
			.BuildAndAddToDb(DbContext);
	}

	private async Task<SupplierProduct> AddSupplierProduct(
		CatalogueCandidate? candidate,
		string sku,
		string producer,
		string name)
	{
		var supplierProduct = new SupplierProductBuilder(Faker)
			.WithSku(sku)
			.WithProducer(producer)
			.WithSupplier(Supplier.FavoritParts)
			.Build();
		supplierProduct.AddName(name);
		candidate?.AddSupplierProduct(supplierProduct);

		await DbContext.AddAsync(supplierProduct);
		await DbContext.SaveChangesAsync();
		return supplierProduct;
	}
}
