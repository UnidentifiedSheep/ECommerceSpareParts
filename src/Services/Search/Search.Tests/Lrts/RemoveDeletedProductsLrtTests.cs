using Application.Common.Consumer;
using Contracts.Products;
using Contracts.Tasks;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Search.Abstractions.Options;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Application.Lrts;
using Search.Application.Models.ProductUpdates;
using Search.Entities;
using Search.Enums;
using Tests.TestContainers.Search;

namespace Search.Tests.Lrts;

public sealed class RemoveDeletedProductsLrtTests(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Consumer_RunsTheRegisteredLrtAcrossMultipleBatches_AndCanRunAgain()
	{
		var optionsCache = Scope.ServiceProvider.GetRequiredService<IOptionsMonitorCache<DeletedProductsCleanupOptions>>();
		optionsCache.Clear();
		optionsCache.TryAdd(Options.DefaultName, new DeletedProductsCleanupOptions { RetentionDays = 7, BatchSize = 2 });
		var now = DateTime.UtcNow;
		await Products.ApplyUpdates(Enumerable.Range(1, 5).Select(id => Core(id, now.AddDays(-8), true)).ToArray(), CancellationToken);
		await Products.ApplyUpdates([Core(6, now.AddDays(-1), true), Core(7, now.AddDays(-30))], CancellationToken);
		await Products.ApplyUpdates([ProductWeightUpdate.From(new ProductWeightSnapshot(8, null), now.AddDays(-30))], CancellationToken);
		var candidates = Scope.ServiceProvider.GetRequiredService<ICatalogueCandidateRepository>();
		var candidateId = Guid.NewGuid();
		await candidates.Upsert(new CatalogueCandidate
		{
			Id = candidateId, Sku = "CAND-1", NormalizedSku = "cand1", ProducerId = 42, MappedProductId = null, Names = ["Candidate"]
		}, CancellationToken);
		await RefreshAsync();
		var context = new Mock<ConsumeContext<ServiceTaskRequestedEvent>>();
		context.SetupGet(value => value.Message).Returns(new ServiceTaskRequestedEvent { TaskSystemName = RemoveDeletedProductsLrt.Name });
		context.SetupGet(value => value.CancellationToken).Returns(CancellationToken);
		var consumer = ActivatorUtilities.CreateInstance<ServiceTaskRequestedConsumer>(Scope.ServiceProvider);

		await consumer.Consume(context.Object);
		await consumer.Consume(context.Object);

		for (var id = 1; id <= 5; id++)
			(await Products.GetById(id, CancellationToken)).Should().BeNull();
		(await Products.GetById(6, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Deleted);
		(await Products.GetById(7, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);
		(await Products.GetById(8, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Partial);
		(await candidates.GetById(candidateId, CancellationToken)).Should().NotBeNull();
	}

	private static ProductCoreUpdate Core(int id, DateTime updatedAt, bool deleted = false) =>
		ProductCoreUpdate.From(new ProductSnapshot
		{
			Id = id, Sku = $"SKU-{id}", Name = $"Product {id}", ProducerId = 42, IsDeleted = deleted
		}, updatedAt);
}
