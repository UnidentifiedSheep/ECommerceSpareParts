using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Contracts.Producer;
using Enums;
using Main.Application.Consumers;
using Main.Entities.Producer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tests.Stubs;
using FluentAssertions;
using Main.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.ServicesTests;

public sealed class ProducerLookupServiceCachingTests : IntegrationTest
{
	public ProducerLookupServiceCachingTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	[Fact]
	public async Task ProducerLookup_LoadFromDifferentScopes_ReturnsSameSnapshot()
	{
		var service = Scope.ServiceProvider.GetRequiredService<IProducerLookupService>();

		var first = await service.Load(CancellationToken);
		await using var otherScope = Scope.ServiceProvider.CreateAsyncScope();
		var otherService = otherScope.ServiceProvider.GetRequiredService<IProducerLookupService>();
		var second = await otherService.Load(CancellationToken);

		second.Should().BeSameAs(first);
	}

	[Fact]
	public async Task Load_IncludesSupplierMappings()
	{
		var producer = GetContext<ProducerTestContext>().Producers[0];
		const string supplierProducerName = "Supplier-specific producer";
		await new ProducerSupplierMappingBuilder(Faker)
			.WithProducerId(producer.Id)
			.WithSupplier(Supplier.Armtek)
			.WithSupplierProducerName(supplierProducerName)
			.BuildAndAddToDb(Context, CancellationToken);

		var service = Scope.ServiceProvider.GetRequiredService<IProducerLookupService>();

		var lookup = await service.Load(CancellationToken);

		lookup.ResolveId($" {supplierProducerName.ToUpperInvariant()} ", Supplier.Armtek).Should().Be(producer.Id);
	}

	[Theory]
	[InlineData("create")]
	[InlineData("rename")]
	[InlineData("delete")]
	[InlineData("add-alias")]
	[InlineData("delete-alias")]
	[InlineData("add-mapping")]
	[InlineData("delete-mapping")]
	public async Task CommittedChanges_PublishInvalidationAndRefreshSnapshot(string change)
	{
		var producer = await new ProducerBuilder(Faker).WithName("Snapshot Original").BuildAndAddToDb(Context, CancellationToken);
		var alias = ProducerAlias.Create(producer.Id, "Snapshot Alias");
		var mapping = ProducerSupplierMapping.Create(producer.Id, "Snapshot Supplier", Supplier.Armtek);
		Context.Add(alias);
		Context.Add(mapping);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		var service = Scope.ServiceProvider.GetRequiredService<IProducerLookupService>();
		var before = await service.Load(CancellationToken);
		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		await transaction.WithSettings(new TransactionalAttribute()).ExecuteAsync(async (context, ct) =>
		{
			switch (change)
			{
				case "create": await context.UnitOfWork.AddAsync(Producer.Create("Snapshot New"), ct); break;
				case "rename": producer.SetName("Snapshot New"); break;
				case "delete":
					context.UnitOfWork.Remove(mapping);
					context.UnitOfWork.Remove(producer);
					break;
				case "add-alias": await context.UnitOfWork.AddAsync(ProducerAlias.Create(producer.Id, "Snapshot New"), ct); break;
				case "delete-alias": context.UnitOfWork.Remove(alias); break;
				case "add-mapping": await context.UnitOfWork.AddAsync(ProducerSupplierMapping.Create(producer.Id, "Snapshot New", Supplier.Armtek), ct); break;
				case "delete-mapping": context.UnitOfWork.Remove(mapping); break;
			}
			await context.UnitOfWork.SaveChangesAsync(ct);
		}, CancellationToken);

		var publisher = (MessageBrokerStub)transaction.Context.Publisher;
		publisher.PublishedMessagesOfType<ProducerLookupChangedEvent>().Should().ContainSingle();
		await new ProducerLookupChangedConsumer(service).Consume(Mock.Of<ConsumeContext<ProducerLookupChangedEvent>>());
		var after = await service.Load(CancellationToken);
		after.Should().NotBeSameAs(before);
		before.ResolveId("Snapshot Original").Should().Be(producer.Id);
		if (change == "rename" || change == "delete")
			after.ResolveId("Snapshot Original").Should().BeNull();
		if (change is "create" or "rename" or "add-alias")
			after.ResolveId("Snapshot New").Should().NotBeNull();
		if (change == "delete-alias")
			after.ResolveId("Snapshot Alias").Should().BeNull();
		if (change == "add-mapping")
			after.ResolveId("Snapshot New", Supplier.Armtek).Should().Be(producer.Id);
		if (change is "delete-mapping" or "delete")
			after.ResolveId("Snapshot Supplier", Supplier.Armtek).Should().BeNull();
	}

	[Fact]
	public async Task RolledBackChange_DoesNotPublishInvalidationOrReplaceSnapshot()
	{
		var producer = GetContext<ProducerTestContext>().Producers[0];
		var originalName = producer.Name;
		var service = Scope.ServiceProvider.GetRequiredService<IProducerLookupService>();
		var before = await service.Load(CancellationToken);
		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();

		await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.WithSettings(new TransactionalAttribute())
			.ExecuteAsync(async (context, ct) =>
			{
				producer.SetName("Rolled Back Name");
				await context.UnitOfWork.SaveChangesAsync(ct);
				throw new InvalidOperationException("Rollback");
			}, CancellationToken));

		((MessageBrokerStub)transaction.Context.Publisher).PublishedMessagesOfType<ProducerLookupChangedEvent>().Should().BeEmpty();
		(await service.Load(CancellationToken)).Should().BeSameAs(before);
		(await Context.Producers.AsNoTracking().SingleAsync(x => x.Id == producer.Id, CancellationToken)).Name.Should().Be(originalName);
	}
}
