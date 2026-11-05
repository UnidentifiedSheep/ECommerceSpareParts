using Application.Common.Extensions;
using Application.Common.Interfaces.History;
using Application.Common.Interfaces.Persistence;
using Application.Common.Services.History;
using Domain;
using Domain.CommonEntities.History;
using Domain.CommonEntities.Job;
using Domain.Keys;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Persistence.Entities;

namespace Tests.Tests.Services.History;

public sealed class EntityHistoryContextTests
{
	[Fact]
	public void Reasons_AreDistinctAndBelongToTheExactEntity()
	{
		var context = new EntityHistoryContext();
		var firstSale = new EntityHistoryReason("Sale", "Sale", "[10]");
		var secondSale = new EntityHistoryReason("Sale", "Sale", "[11]");
		var id = Guid.NewGuid();
		var otherId = Guid.NewGuid();
		context.AddReason<TestMetadataEntity, Guid>(id, firstSale);
		context.AddReason<TestMetadataEntity, Guid>(id, firstSale);
		context.AddReason<TestMetadataEntity, Guid>(id, secondSale);
		context.AddReason<TestAuditableMetadataEntity, Guid>(id, new EntityHistoryReason("OrderCancelled"));
		context.AddReason<TestMetadataEntity, Guid>(otherId, new EntityHistoryReason("InventoryCorrection"));

		context.GetReasons<TestMetadataEntity, Guid>(id).Should().BeEquivalentTo([firstSale, secondSale]);
		context.GetReasons<TestAuditableMetadataEntity, Guid>(id).Should().Equal(new EntityHistoryReason("OrderCancelled"));
		context.GetReasons<TestMetadataEntity, Guid>(otherId).Should().Equal(new EntityHistoryReason("InventoryCorrection"));
		context.GetReasons<TestMetadataEntity, Guid>(Guid.NewGuid()).Should().BeEmpty();
	}

	[Fact]
	public void ReadingReasons_DoesNotExposeTheStoredCollection()
	{
		var context = new EntityHistoryContext();
		var entity = new TestMetadataEntityBuilder().Build();
		var reason = new EntityHistoryReason("OrderConfirmed");
		context.AddReason(entity, reason);
		var snapshot = context.GetReasons(entity);

		((EntityHistoryReason[])snapshot)[0] = new EntityHistoryReason("ChangedByCaller");
		context.GetReasons<TestMetadataEntity, Guid>(entity.Id).Should().Equal(reason);
		context.Clear();
		context.GetReasons(entity).Should().BeEmpty();
	}

	[Fact]
	public void TupleKeys_KeepComponentsSeparate()
	{
		var context = new EntityHistoryContext();
		var reason = new EntityHistoryReason("QuantityChanged");
		context.AddReason<TupleEntity, (int, string)>((12, "3,4"), reason);

		context.GetReasons<TupleEntity, (int, string)>((12, "3,4")).Should().Equal(reason);
		context.GetReasons<TupleEntity, (int, string)>((123, ",4")).Should().BeEmpty();
		context.GetReasons<TupleEntity, (int, string)>((12, "3")).Should().BeEmpty();
	}

	[Fact]
	public void CompositeKeyReasonsAndSources_UseTheSharedKeyFormat()
	{
		var context = new EntityHistoryContext();
		var stepId = Guid.NewGuid();
		var dependencyId = Guid.NewGuid();
		var key = new JobStepDependencyKey(stepId, dependencyId);
		var reason = EntityHistoryReason.ForSource<JobStepDependency, JobStepDependencyKey>("DependencyChanged", key);
		context.AddReason<JobStepDependency, JobStepDependencyKey>(key, reason);

		context.GetReasons<JobStepDependency, JobStepDependencyKey>(new JobStepDependencyKey(stepId, dependencyId)).Should().Equal(reason);
		reason.SourceType.Should().Be(nameof(JobStepDependency));
		reason.SourceKey.Should().Be(EntityKey.FormatValues([stepId, dependencyId]));
	}

	[Fact]
	public void EntitySourceOverload_InfersTheEntityAndKeyTypes()
	{
		var entity = new TestMetadataEntityBuilder().Build();
		var reason = EntityHistoryReason.ForSource("Changed", entity);

		reason.Should().Be(EntityHistoryReason.ForSource<TestMetadataEntity, Guid>("Changed", entity.Id));
		reason.SourceType.Should().Be(nameof(TestMetadataEntity));
		reason.SourceKey.Should().Be(EntityKey.Format(entity.Id));
	}

	[Fact]
	public async Task RequestAndFactoryScopes_HaveIndependentHistoryContexts()
	{
		var services = new ServiceCollection();
		services.RegisterApplicationTransactions();
		await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
		await using var scope = provider.CreateAsyncScope();
		var request = scope.ServiceProvider.GetRequiredService<IEntityHistoryContext>();
		var entity = new TestMetadataEntityBuilder().Build();
		request.AddReason(entity, new EntityHistoryReason("Request"));
		scope.ServiceProvider.GetRequiredService<IEntityHistoryContext>().Should().BeSameAs(request);

		var factory = provider.GetRequiredService<IApplicationTransactionFactory>();
		await using var first = factory.Create();
		await using var second = factory.Create();
		var firstHistory = first.Context.GetRequiredService<IEntityHistoryContext>();
		var secondHistory = second.Context.GetRequiredService<IEntityHistoryContext>();
		firstHistory.AddReason(entity, new EntityHistoryReason("FirstBatch"));

		firstHistory.Should().NotBeSameAs(request);
		secondHistory.Should().NotBeSameAs(firstHistory);
		secondHistory.GetReasons(entity).Should().BeEmpty();
		request.GetReasons(entity).Should().Equal(new EntityHistoryReason("Request"));
	}

	private sealed class TupleEntity : Entity<TupleEntity, (int, string)>
	{
		public override (int, string) GetId() => (12, "3,4");
	}
}
