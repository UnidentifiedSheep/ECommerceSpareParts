using Application.Common.Services.Events;
using Domain.Interfaces.Events;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence.Interceptors;
using Persistence.Services;
using Tests.Extensions;
using Tests.Persistence.Context;
using Tests.Persistence.Entities;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Persistence;

public sealed class BeforeSaveDomainEventTests(CombinedContainerFixture fixture) : IAsyncLifetime
{
	private DContext _context = null!;
	private Func<object, Task> _handle = _ => Task.CompletedTask;
	private TestEnvironmentLease _lease = null!;
	private DomainEventScope _scope = null!;
	private CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	public async ValueTask InitializeAsync()
	{
		_lease = await fixture.AcquireAsync(CancellationToken);
		var publisher = new Mock<IPublisher>();
		publisher.Setup(item => item.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Returns((object item, CancellationToken _) => _handle(item));
		_scope = new DomainEventScope();
		var collector = new DomainEventCollector<DContext>(_scope, () => _context);
		var executor = new DomainEventExecutor(
			_scope,
			publisher.Object,
			collector);
		_context = new DContext(
			new DbContextOptionsBuilder<DContext>()
				.UseNpgsql(_lease.Slot.PostgresConnectionString)
				.AddInterceptors(
					new DomainEventFlusherSaveChangesInterceptor(
						collector,
						() => executor))
				.Options);
		await _context.Database.EnsureCreatedAsync(CancellationToken);
	}

	public async ValueTask DisposeAsync()
	{
		try { await _context.ClearDatabase(CancellationToken); }
		finally
		{
			await _context.DisposeAsync();
			await _lease.DisposeAsync();
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task BeforeSave_ModifiesCurrentWriteAndDefersOrdinaryEvents(bool batch)
	{
		using var collection = _scope.EnableCollection();
		var entity = new BeforeSaveTestEntity();
		entity.Raise(batch ? new BatchBeforeEvent(entity) : new BeforeEvent(entity));
		entity.Raise(new OrdinaryEvent());
		_context.Add(entity);
		_handle = notification =>
		{
			var target = notification switch
			{
				BeforeEvent single => single.Entity,
				Batch<BatchBeforeEvent> items => items.Items.Single().Entity,
				_ => throw new InvalidOperationException("Ordinary event dispatched before SQL")
			};
			target.Value = "prepared";
			_context.Add(new BeforeSaveTestEntity { Value = "added by handler" });
			return Task.CompletedTask;
		};

		await _context.SaveChangesAsync(CancellationToken);
		_context.ChangeTracker.Clear();

		var values = await _context.Set<BeforeSaveTestEntity>()
			.Select(item => item.Value)
			.ToListAsync(CancellationToken);
		values.Should().BeEquivalentTo("prepared", "added by handler");
		entity.CreatedCalls.Should().Be(1);
		_scope.Flush().Should().ContainSingle().Which.Should().BeOfType<OrdinaryEvent>();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task HandlerFailureOrNestedSave_PreventsSqlAndAllowsRetry(bool nestedSave)
	{
		var entity = new BeforeSaveTestEntity();
		entity.Raise(new BeforeEvent(entity));
		_context.Add(entity);
		_handle = async _ =>
		{
			if (nestedSave) await _context.SaveChangesAsync(CancellationToken);
			else throw new InvalidOperationException("Handler failed");
		};

		await Assert.ThrowsAsync<InvalidOperationException>(()
			=> _context.SaveChangesAsync(CancellationToken));
		(await _context.Set<BeforeSaveTestEntity>().CountAsync(CancellationToken)).Should().Be(0);
		_scope.Flush().Should().BeEmpty();

		entity.Raise(new BeforeEvent(entity));
		_handle = _ =>
		{
			entity.Value = "retried";
			return Task.CompletedTask;
		};
		await _context.SaveChangesAsync(CancellationToken);
		_context.ChangeTracker.Clear();
		(await _context.Set<BeforeSaveTestEntity>().SingleAsync(CancellationToken)).Value
			.Should()
			.Be("retried");
	}

	[Fact]
	public async Task HandlerRaisesEntityEvent_NextRoundCollectsItWithoutRepeatingLifecycle()
	{
		var entity = new BeforeSaveTestEntity();
		entity.Raise(new BeforeEvent(entity));
		_context.Add(entity);
		var calls = 0;
		_handle = _ =>
		{
			if (++calls == 1) entity.Raise(new BeforeEvent(entity));
			return Task.CompletedTask;
		};

		await _context.SaveChangesAsync(CancellationToken);

		calls.Should().Be(2);
		entity.CreatedCalls.Should().Be(1);
	}

	[Fact]
	public async Task SynchronousSave_IsNotSupportedButDoesNotPreventAsyncSave()
	{
		var entity = new BeforeSaveTestEntity();
		entity.Raise(new BeforeEvent(entity));
		_context.Add(entity);
		Assert.Throws<NotSupportedException>(() => _context.SaveChanges());
		_handle = _ =>
		{
			entity.Value = "handled";
			return Task.CompletedTask;
		};

		await _context.SaveChangesAsync(CancellationToken);
		_context.ChangeTracker.Clear();

		(await _context.Set<BeforeSaveTestEntity>().SingleAsync(CancellationToken)).Value
			.Should()
			.Be("handled");
	}

	private sealed record BeforeEvent(BeforeSaveTestEntity Entity) : IBeforeSaveDomainEvent;

	private sealed record BatchBeforeEvent(BeforeSaveTestEntity Entity)
		: IBeforeSaveDomainEvent, IBatchableDomainEvent;

	private sealed record OrdinaryEvent : IDomainEvent;
}
