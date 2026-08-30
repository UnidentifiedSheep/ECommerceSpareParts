using Bogus;
using Main.Entities.Producer;
using Main.Entities.Product.Enrichment;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public sealed class CatalogueCandidateBuilder(Faker faker) : BuilderBase<CatalogueCandidate>(faker)
{
	private readonly HashSet<int> _producerIds = [];

	public string? Sku { get; private set; }

	public int? ProductId { get; private set; }

	public IReadOnlySet<int> ProducerIds => _producerIds;

	public CatalogueCandidateBuilder WithSku(string sku)
	{
		Sku = sku;
		return this;
	}

	public CatalogueCandidateBuilder WithProducerId(int producerId)
	{
		_producerIds.Add(producerId);
		return this;
	}

	public CatalogueCandidateBuilder WithProducerIds(IEnumerable<int> producerIds)
	{
		_producerIds.UnionWith(producerIds);
		return this;
	}

	public CatalogueCandidateBuilder WithProducers(IEnumerable<Producer> producers) =>
		WithProducerIds(producers.Select(x => x.Id));

	public CatalogueCandidateBuilder WithProductId(int productId)
	{
		ProductId = productId;
		return this;
	}

	public override CatalogueCandidate Build()
	{
		var candidate = CatalogueCandidate.Create(
			Sku ?? Faker.Random.AlphaNumeric(12),
			Faker.PickRandom<int>(_producerIds));

		if (ProductId.HasValue)
			candidate.MapToProduct(ProductId.Value);

		return candidate;
	}
}
