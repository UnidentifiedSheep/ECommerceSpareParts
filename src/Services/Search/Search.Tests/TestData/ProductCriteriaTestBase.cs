using Application.Common.Models;
using Contracts.Products;
using Enums.Units;
using Search.Application.Handlers.Search.Catalogue;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Application.Models.ProductUpdates;
using Search.Enums;
using Tests.TestContainers.Search;

namespace Search.Tests.TestData;

public abstract class ProductCriteriaTestBase(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();
		var updatedAt = DateTime.UtcNow;
		await Products.ApplyUpdates(new ProductSnapshot[]
		{
			new() { Id = 101, Sku = "OIL-101", Name = "Oil filter", ProducerId = 42, ProductGroupId = 7, Stock = 5 },
			new() { Id = 102, Sku = "AIR-102", Name = "Air filter", ProducerId = 42, ProductGroupId = 7, Stock = 0 },
			new() { Id = 103, Sku = "FUEL-103", Name = "Fuel filter", ProducerId = 43, ProductGroupId = 8, Stock = 10 },
			new() { Id = 105, IsDeleted = true }
		}.Select(product => ProductCoreUpdate.From(product, updatedAt)).ToArray(), CancellationToken);
		await Products.ApplyUpdates(new ProductCharacteristicsSnapshot[]
		{
			new(101, [new(10, "12.5"), new(20, "M10"), new(30, "steel"), new(40, "-20")]),
			new(102, [new(10, "100"), new(20, "M12"), new(99, "12.5")]),
			new(103, [new(10, "not a number"), new(20, "M12")]),
			new(104, [new(10, "12.5"), new(20, "M10")]),
			new(105, [new(10, "12.5"), new(20, "M10")])
		}.Select(product => ProductCharacteristicsUpdate.From(product, updatedAt)).ToArray(), CancellationToken);
		await Products.ApplyUpdates(new ProductDimensionsSnapshot[]
		{
			new(101, new(10, 20, 30, DimensionUnit.Millimeter)),
			new(102, new(2, 2, 3, DimensionUnit.Centimeter))
		}.Select(product => ProductDimensionsUpdate.From(product, updatedAt)).ToArray(), CancellationToken);
		await Products.ApplyUpdates(new ProductWeightSnapshot[]
		{
			new(101, new(2000, WeightUnit.Gram)), new(102, new(3, WeightUnit.Kilogram))
		}.Select(product => ProductWeightUpdate.From(product, updatedAt)).ToArray(), CancellationToken);
		await RefreshAsync();
	}

	protected async Task<int[]> FindAsync(ProductSearchCriteria criteria, string? query = null)
	{
		var result = await Mediator.Send(new SearchInCatalogueQuery(
			query, new HashSet<SearchTarget> { SearchTarget.Products }, new HashSet<SearchMatchType>(),
			string.IsNullOrEmpty(query) ? new HashSet<SearchMatchType>() : new HashSet<SearchMatchType> { SearchMatchType.StartsWith },
			[], new Pagination(0, 20), [], [], ProductCriteria: criteria), CancellationToken);
		return result.Products.Items.Select(product => product.Id).Order().ToArray();
	}
}
