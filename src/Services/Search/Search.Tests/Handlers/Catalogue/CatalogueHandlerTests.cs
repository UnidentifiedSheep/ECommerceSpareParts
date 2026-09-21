using Abstractions.Models;
using FluentAssertions;
using Moq;
using Search.Application.Handlers.Search.Catalogue;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Application.Interfaces.Product;
using Search.Application.Models.CatalogueSearch;
using Search.Application.Projections;
using Search.Enums;
using CatalogueCandidateDocument = Search.Entities.CatalogueCandidate;
using ProductDocument = Search.Entities.Product;

namespace Search.Tests.Handlers.Catalogue;

public sealed class CatalogueHandlerTests
{
	private readonly Mock<ICatalogueCandidateRepository> _candidateRepository = new();

	private readonly Mock<IProductRepository> _productRepository = new();

	[Fact]
	public async Task Handle_WhenBothTargetsRequested_ShouldReturnGroupedResults()
	{
		var product = CreateProduct();
		var candidate = CreateCandidate();
		CatalogueSearchCriteria? receivedCriteria = null;
		CatalogueSearchCriteria? receivedCandidateCriteria = null;
		_productRepository
			.Setup(x => x.Search(It.IsAny<CatalogueSearchCriteria>(), It.IsAny<CancellationToken>()))
			.Callback<CatalogueSearchCriteria,
				CancellationToken>((criteria, _) => receivedCriteria = criteria)
			.ReturnsAsync(
				new SearchResult<ProductDocument>(
					[
						new SearchHit<ProductDocument>(
							product,
							new Dictionary<string, IReadOnlyCollection<string>>())
					],
					11));
		_candidateRepository
			.Setup(x => x.Search(It.IsAny<CatalogueSearchCriteria>(), It.IsAny<CancellationToken>()))
			.Callback<CatalogueSearchCriteria,
				CancellationToken>((criteria, _) => receivedCandidateCriteria = criteria)
			.ReturnsAsync(
				new SearchResult<CatalogueCandidateDocument>(
					[
						new SearchHit<CatalogueCandidateDocument>(
							candidate,
							new Dictionary<string, IReadOnlyCollection<string>>())
					],
					7));
		var handler = CreateHandler();

		var result = await handler.Handle(
			CreateQuery(
				new HashSet<SearchTarget>
				{
					SearchTarget.Products, SearchTarget.CatalogueCandidates
				}),
			CancellationToken.None);

		result.Products.Total.Should().Be(11);
		result.Products.Items.Should().ContainSingle().Which.Id.Should().Be(product.Id);
		result.CatalogueCandidates.Total.Should().Be(7);
		result.CatalogueCandidates.Items.Should().ContainSingle().Which.Id.Should().Be(candidate.Id);
		result.CatalogueCandidates.Items.Single().MappedProductId.Should().Be(candidate.MappedProductId);
		receivedCriteria.Should().NotBeNull();
		receivedCriteria!.Query.Should().Be("bosch 123");
		receivedCriteria.ProducerIds.Should().Equal(42);
		receivedCandidateCriteria.Should().NotBeNull();
		receivedCandidateCriteria!.CandidateMappingStatus.Should().Be(CandidateMappingStatus.Unmapped);
	}

	[Fact]
	public async Task Handle_WhenHighlightsIncluded_ShouldMapThemToItems()
	{
		var product = CreateProduct();
		var highlights = new Dictionary<string, IReadOnlyCollection<string>>
		{
			["name"] = ["Product [[[name]]]"]
		};
		_productRepository
			.Setup(x => x.Search(It.IsAny<CatalogueSearchCriteria>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(
				new SearchResult<ProductDocument>([new SearchHit<ProductDocument>(product, highlights)], 1));
		var handler = CreateHandler();

		var result = await handler.Handle(
			CreateQuery(
				new HashSet<SearchTarget>
				{
					SearchTarget.Products
				},
				true),
			CancellationToken.None);

		result.Products.Items.Single().Highlights.Should().BeEquivalentTo(highlights);
	}

	[Fact]
	public async Task Handle_WhenOnlyCandidatesRequested_ShouldNotSearchProducts()
	{
		_candidateRepository
			.Setup(x => x.Search(It.IsAny<CatalogueSearchCriteria>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new SearchResult<CatalogueCandidateDocument>([], 0));
		var handler = CreateHandler();

		var result = await handler.Handle(
			CreateQuery(
				new HashSet<SearchTarget>
				{
					SearchTarget.CatalogueCandidates
				}),
			CancellationToken.None);

		result.Products.Items.Should().BeEmpty();
		result.Products.Total.Should().Be(0);
		_productRepository.Verify(
			x => x.Search(It.IsAny<CatalogueSearchCriteria>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_candidateRepository.Verify(
			x => x.Search(It.IsAny<CatalogueSearchCriteria>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	private CatalogueHandler CreateHandler()
	{
		var productProjection = new ProductDtoProjectionProvider(
			new ProductDimensionsDtoProjectionProvider(),
			new ProductWeightDtoProjectionProvider());

		return new CatalogueHandler(
			_productRepository.Object,
			_candidateRepository.Object,
			productProjection,
			new CatalogueCandidateDtoProjectionProvider());
	}

	private static CatalogueQuery CreateQuery(
		IReadOnlySet<SearchTarget> targets,
		bool includeHighlights = false,
		CandidateMappingStatus candidateMappingStatus = CandidateMappingStatus.Unmapped)
	{
		return new CatalogueQuery(
			"  bosch 123  ",
			targets,
			new HashSet<SearchMatchType>
			{
				SearchMatchType.Exact
			},
			new HashSet<SearchMatchType>
			{
				SearchMatchType.StartsWith
			},
			[42, 42],
			new Pagination(0, 20),
			[],
			[],
			candidateMappingStatus,
			includeHighlights);
	}

	private static ProductDocument CreateProduct()
	{
		return new ProductDocument
		{
			Id = 123,
			Sku = "BOSCH-123",
			NormalizedSku = "bosch123",
			Name = "Product name",
			ProducerId = 42,
			Stock = 5,
			Indicator = null
		};
	}

	private static CatalogueCandidateDocument CreateCandidate()
	{
		return new CatalogueCandidateDocument
		{
			Id = Guid.NewGuid(),
			Sku = "BOSCH-123",
			NormalizedSku = "bosch123",
			ProducerId = 42,
			MappedProductId = null,
			Names = ["Candidate name"]
		};
	}
}
