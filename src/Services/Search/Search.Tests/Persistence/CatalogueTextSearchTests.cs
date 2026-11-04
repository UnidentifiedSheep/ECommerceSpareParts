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
}
