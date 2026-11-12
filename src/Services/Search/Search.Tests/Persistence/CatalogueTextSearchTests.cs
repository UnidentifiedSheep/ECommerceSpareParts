using Application.Common.Models;
using FluentAssertions;
using Search.Application.Models.CatalogueSearch;
using Search.Application.Models.ProductUpdates;
using Search.Entities;
using Search.Enums;
using Tests.TestContainers.Search;

namespace Search.Tests.Persistence;

public sealed class CatalogueTextSearchTests(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	[Theory]
	[InlineData("bosch", true)]
	[InlineData("bosch", false)]
	[InlineData("ab12", true)]
	[InlineData("ab12", false)]
	public async Task TextSearch_WithCurrentMapping_ReturnsProduct(string query, bool highlights)
	{
		await Products.ApplyUpdates([new ProductCoreUpdate
		{
			ProductId = 123, UpdatedAt = DateTime.UtcNow,
			Sku = "AB-12345", NormalizedSku = "ab12345", Name = "Bosch oil filter", ProducerId = 1,
			ReadinessStatus = ProductReadinessStatus.Ready
		}], CancellationToken);
		await RefreshAsync();

		var result = await Products.Search(new CatalogueSearchCriteria
		{
			Query = query, SkuModes = CatalogueSearchDefaults.SkuModes, NameModes = CatalogueSearchDefaults.NameModes,
			Pagination = new Pagination(0, 20), IncludeHighlights = highlights
		}, CancellationToken);

		result.Hits.Should().ContainSingle().Which.Document.Id.Should().Be(123);
		if (highlights && query == "bosch")
			result.Hits.Single().Highlights.Should().ContainKey("name");
		if (!highlights)
			result.Hits.Single().Highlights.Should().BeEmpty();
	}
	private const string ExhaustBellowsName =
		"Гофра глушителя D:121 L:1000 о.н MAN  81152100033 / Dinex 94120 (материал - цинк)";

	[Theory]
	[InlineData("гофра 121")]
	[InlineData("ГОФРА 121")]
	public async Task NameContains_SeparatedTerms_ReturnsProductContainingBothTerms(string query)
	{
		await SeedExhaustProductsAsync();

		var result = await Products.Search(NameContainsCriteria(query), CancellationToken);

		result.Hits.Should().ContainSingle().Which.Document.Id.Should().Be(123);
	}

	[Theory]
	[InlineData("гофра глушителя")]
	[InlineData("глушителя D:121")]
	[InlineData("81152100033 / Dinex 94120")]
	public async Task NameContains_ContiguousSubstring_ReturnsProduct(string query)
	{
		await SeedExhaustProductsAsync();

		var result = await Products.Search(NameContainsCriteria(query), CancellationToken);

		result.Hits.Should().ContainSingle().Which.Document.Id.Should().Be(123);
	}

	[Theory]
	[InlineData("гофра 122")]
	[InlineData("шланг 121")]
	public async Task NameContains_MissingRequestedTerm_DoesNotReturnProducts(string query)
	{
		await SeedExhaustProductsAsync();

		var result = await Products.Search(NameContainsCriteria(query), CancellationToken);

		result.Hits.Should().BeEmpty();
	}

	[Fact]
	public async Task NameAnalyzer_SeparatesDiameterNumberFromLabel()
	{
		var result = await Client.Indices.AnalyzeAsync(request => request.Index(Indexes.Products)
			.Field("name").Text(ExhaustBellowsName), CancellationToken);

		result.IsValid.Should().BeTrue(result.DebugInformation);
		result.Tokens.Select(token => token.Token).Should().Contain(["гофра", "121", "1000"]);
	}

	private async Task SeedExhaustProductsAsync()
	{
		var now = DateTime.UtcNow;
		await Products.ApplyUpdates([
			new ProductCoreUpdate
			{
				ProductId = 123, UpdatedAt = now, Sku = "EXH-A", NormalizedSku = "exha",
				Name = ExhaustBellowsName, ProducerId = 1, ReadinessStatus = ProductReadinessStatus.Ready
			},
			new ProductCoreUpdate
			{
				ProductId = 124, UpdatedAt = now, Sku = "EXH-B", NormalizedSku = "exhb",
				Name = "Гофра D:110 L:1000", ProducerId = 1, ReadinessStatus = ProductReadinessStatus.Ready
			},
			new ProductCoreUpdate
			{
				ProductId = 125, UpdatedAt = now, Sku = "EXH-C", NormalizedSku = "exhc",
				Name = "Труба D:121 L:1000", ProducerId = 1, ReadinessStatus = ProductReadinessStatus.Ready
			}
		], CancellationToken);
		await RefreshAsync();
	}

	private static CatalogueSearchCriteria NameContainsCriteria(string query) => new()
	{
		Query = query, SkuModes = new HashSet<SearchMatchType>(), NameModes = new HashSet<SearchMatchType> { SearchMatchType.Contains },
		Pagination = new Pagination(0, 20)
	};

}
