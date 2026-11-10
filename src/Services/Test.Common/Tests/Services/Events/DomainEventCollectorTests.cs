using Application.Common.Services.Events;
using Domain;
using Domain.Interfaces.Events;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence.Services;

namespace Tests.Tests.Services.Events;

public sealed class DomainEventCollectorTests
{
	[Theory]
	[InlineData(EntityState.Added, "created")]
	[InlineData(EntityState.Modified, "updated")]
	[InlineData(EntityState.Deleted, "deleted")]
	public void RepeatedCollection_DoesNotRepeatLifecycleCallbacks(EntityState state, string expected)
	{
		using var context = CreateContext();
		var scope = new DomainEventScope(Mock.Of<IPublisher>());
		using var collection = scope.EnableCollection();
		var collector = new DomainEventCollector(scope);
		var entity = new CollectorEntity();
		context.Entry(entity).State = state;

		collector.Collect(context);
		collector.Collect(context);

		entity.Callbacks.Should().Equal(expected);
		scope.Flush().Should().ContainSingle().Which.Should().Be(new LifecycleEvent(expected));
	}

	[Fact]
	public void RepeatedCollection_StillCollectsNewExplicitEvents()
	{
		using var context = CreateContext();
		var scope = new DomainEventScope(Mock.Of<IPublisher>());
		using var collection = scope.EnableCollection();
		var collector = new DomainEventCollector(scope);
		var entity = new CollectorEntity();
		context.Add(entity);
		collector.Collect(context);
		scope.Flush();
		entity.Raise(new ExplicitEvent());

		collector.Collect(context);

		entity.Callbacks.Should().Equal("created");
		scope.Flush().Should().ContainSingle().Which.Should().BeOfType<ExplicitEvent>();
	}

	[Fact]
	public void Reset_AllowsLifecycleCallbackForTheNextSave()
	{
		using var context = CreateContext();
		var scope = new DomainEventScope(Mock.Of<IPublisher>());
		using var collection = scope.EnableCollection();
		var collector = new DomainEventCollector(scope);
		var entity = new CollectorEntity();
		context.Attach(entity);
		entity.Value = "first change";
		collector.Collect(context);
		context.ChangeTracker.AcceptAllChanges();
		collector.Reset(context);

		entity.Value = "next change";
		collector.Collect(context);

		entity.Callbacks.Should().Equal("updated", "updated");
		scope.Flush().Should().HaveCount(2);
	}

	[Fact]
	public void ChangedEntityState_RaisesTheMatchingLifecycleCallback()
	{
		using var context = CreateContext();
		var scope = new DomainEventScope(Mock.Of<IPublisher>());
		using var collection = scope.EnableCollection();
		var collector = new DomainEventCollector(scope);
		var entity = new CollectorEntity();
		context.Attach(entity);
		entity.Value = "changed";
		collector.Collect(context);

		context.Remove(entity);
		collector.Collect(context);
		collector.Collect(context);

		entity.Callbacks.Should().Equal("updated", "deleted");
	}

	[Fact]
	public void NextCollection_DiscoversNewEntities()
	{
		using var context = CreateContext();
		var scope = new DomainEventScope(Mock.Of<IPublisher>());
		using var collection = scope.EnableCollection();
		var collector = new DomainEventCollector(scope);
		var first = new CollectorEntity();
		context.Add(first);
		collector.Collect(context);

		var second = new CollectorEntity();
		context.Add(second);
		collector.Collect(context);

		first.Callbacks.Should().Equal("created");
		second.Callbacks.Should().Equal("created");
		scope.Flush().Should().HaveCount(2);
	}

	[Fact]
	public void OutsideCollection_PreservesBeforeSaveEventsWithoutDispatchingThem()
	{
		using var context = CreateContext();
		var publisher = new Mock<IPublisher>(MockBehavior.Strict);
		var scope = new DomainEventScope(publisher.Object);
		var collector = new DomainEventCollector(scope);
		var entity = new CollectorEntity();
		entity.Raise(new BeforeSaveEvent());
		context.Add(entity);

		collector.Collect(context);

		scope.Flush().Should().ContainSingle().Which.Should().BeOfType<BeforeSaveEvent>();
		entity.FlushDomainEvents().Should().BeEmpty();
		publisher.VerifyNoOtherCalls();
	}

	[Fact]
	public void Reset_OnlyResetsTheSpecifiedContext()
	{
		using var first = CreateContext();
		using var second = CreateContext();
		var scope = new DomainEventScope(Mock.Of<IPublisher>());
		var collector = new DomainEventCollector(scope);
		var firstEntity = new CollectorEntity();
		var secondEntity = new CollectorEntity();
		first.Add(firstEntity);
		second.Add(secondEntity);
		collector.Collect(first);
		collector.Collect(second);

		collector.Reset(first);
		collector.Collect(first);
		collector.Collect(second);

		firstEntity.Callbacks.Should().Equal("created", "created");
		secondEntity.Callbacks.Should().Equal("created");
	}

	private static CollectorContext CreateContext() => new(new DbContextOptionsBuilder<CollectorContext>()
		.UseNpgsql("Host=127.0.0.1;Port=1;Database=collector_tests;Username=test;Password=test").Options);

	private sealed class CollectorContext(DbContextOptions<CollectorContext> options) : DbContext(options)
	{
		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<CollectorEntity>().HasKey(entity => entity.Id);
			builder.Entity<CollectorEntity>().Ignore(entity => entity.Callbacks);
		}
	}

	private sealed class CollectorEntity : Entity<CollectorEntity, Guid>
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Value { get; set; } = "original";
		public List<string> Callbacks { get; } = [];
		public override Guid GetId() => Id;
		public void Raise(IDomainEvent item) => AddDomainEvent(item);
		public override void OnCreated() => Notify("created");
		public override void OnUpdated() => Notify("updated");
		public override void OnDeleted() => Notify("deleted");
		private void Notify(string name) { Callbacks.Add(name); AddDomainEvent(new LifecycleEvent(name)); }
	}

	private sealed record LifecycleEvent(string Name) : IDomainEvent;
	private sealed record ExplicitEvent : IDomainEvent;
	private sealed record BeforeSaveEvent : IBeforeSaveDomainEvent;
}
