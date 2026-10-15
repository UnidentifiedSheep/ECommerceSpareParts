using Contracts.Products;
using FluentAssertions;
using Search.Application.Models.ProductUpdates;
using Search.Entities;
using Search.Enums;
using Tests.TestContainers.Search;

namespace Search.Tests.Persistence;

public sealed class DeletedProductCleanupTests(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task DeleteDeletedBefore_DeletesExpiredDeletedDocumentsIncludingTheBoundary_AndKeepsOtherStatuses()
	{
		var cutoff = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
		await Products.ApplyUpdates(
		[
			Core(1, cutoff.AddSeconds(-1), true), Core(2, cutoff, true),
			Core(3, cutoff.AddSeconds(1), true), Core(4, cutoff.AddDays(-30))
		], CancellationToken);
		await Products.ApplyUpdates([ProductWeightUpdate.From(new ProductWeightSnapshot(5, null), cutoff.AddDays(-30))], CancellationToken);
		await RefreshAsync();

		(await Products.DeleteDeletedBefore(cutoff, 100, CancellationToken)).Should().Be(2);

		(await Products.GetById(1, CancellationToken)).Should().BeNull();
		(await Products.GetById(2, CancellationToken)).Should().BeNull();
		(await Products.GetById(3, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Deleted);
		(await Products.GetById(4, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);
		(await Products.GetById(5, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Partial);
	}

	[Fact]
	public async Task DeleteDeletedBefore_PreservesARestoredProductEvenWhenItsCoreTimestampIsBeforeTheCutoff()
	{
		var cutoff = DateTime.UtcNow;
		await Products.ApplyUpdates([Core(1, cutoff.AddDays(-20), true)], CancellationToken);
		await Products.ApplyUpdates([Core(1, cutoff.AddDays(-10))], CancellationToken);
		await RefreshAsync();

		(await Products.DeleteDeletedBefore(cutoff, 100, CancellationToken)).Should().Be(0);

		(await Products.GetById(1, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);
	}

	[Fact]
	public async Task DeleteDeletedBefore_LimitsEachBatchAndMakesProgressUntilTheIndexIsClean()
	{
		var cutoff = DateTime.UtcNow;
		await Products.ApplyUpdates(Enumerable.Range(1, 5).Select(id => Core(id, cutoff.AddDays(-1), true)).ToArray(), CancellationToken);
		await RefreshAsync();

		(await Products.DeleteDeletedBefore(cutoff, 2, CancellationToken)).Should().Be(2);
		(await Products.DeleteDeletedBefore(cutoff, 2, CancellationToken)).Should().Be(2);
		(await Products.DeleteDeletedBefore(cutoff, 2, CancellationToken)).Should().Be(1);
		(await Products.DeleteDeletedBefore(cutoff, 2, CancellationToken)).Should().Be(0);

		for (var id = 1; id <= 5; id++)
			(await Products.GetById(id, CancellationToken)).Should().BeNull();
	}

	[Fact]
	public async Task DeleteDeletedBefore_SkipsAVersionConflictAndPreservesTheNewerReadyDocument()
	{
		var settings = await Client.Indices.UpdateSettingsAsync(Indexes.Products,
			request => request.IndexSettings(index => index.RefreshInterval("-1")), CancellationToken);
		settings.IsValid.Should().BeTrue(settings.DebugInformation);
		var cutoff = DateTime.UtcNow;
		await Products.ApplyUpdates([Core(1, cutoff.AddDays(-2), true)], CancellationToken);
		await RefreshAsync();
		await Products.ApplyUpdates([Core(1, cutoff.AddDays(-1))], CancellationToken);

		// Keep the search snapshot on the old deleted version while realtime GET sees the restored document.
		var stale = await Client.SearchAsync<Product>(
			request => request.Index(Indexes.Products).Query(query => query.Ids(ids => ids.Values(1))),
			CancellationToken);
		stale.IsValid.Should().BeTrue(stale.DebugInformation);
		stale.Documents.Should().ContainSingle().Which.ReadinessStatus.Should().Be(ProductReadinessStatus.Deleted);
		(await Products.GetById(1, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);

		(await Products.DeleteDeletedBefore(cutoff, 100, CancellationToken)).Should().Be(0);

		(await Products.GetById(1, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);
	}

	private static ProductCoreUpdate Core(int id, DateTime updatedAt, bool deleted = false) =>
		ProductCoreUpdate.From(new ProductSnapshot
		{
			Id = id, Sku = $"SKU-{id}", Name = $"Product {id}", ProducerId = 42, IsDeleted = deleted
		}, updatedAt);
}
