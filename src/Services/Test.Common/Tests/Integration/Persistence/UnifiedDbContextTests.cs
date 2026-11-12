using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Domain;
using Domain.Interfaces.Events;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Persistence.Context;
using Persistence.Extensions;
using Persistence.Interceptors;
using Persistence.Interfaces;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Persistence;

public sealed class UnifiedDbContextTests(CombinedContainerFixture fixture) : IAsyncLifetime
{
	private TestEnvironmentLease _lease = null!;
	private ServiceProvider _provider = null!;
	private AsyncServiceScope _scope;
	private Func<object, CancellationToken, Task> _handle = (_, _) => Task.CompletedTask;
	private CancellationToken CancellationToken => TestContext.Current.CancellationToken;
	private IServiceProvider Services => _scope.ServiceProvider;
	private UnifiedDbContext Context => Services.GetRequiredService<UnifiedDbContext>();
	private IUnitOfWork UnitOfWork => Services.GetRequiredService<IUnitOfWork>();

	public async ValueTask InitializeAsync()
	{
		_lease = await fixture.AcquireAsync(CancellationToken);
		var services = new ServiceCollection();
		services.RegisterDomainEventScope().RegisterIntegrationEventScope();
		var publisher = new Mock<IPublisher>();
		publisher.Setup(item => item.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Returns((object item, CancellationToken ct) => _handle(item, ct));
		services.AddSingleton(publisher.Object);
		services.AddScoped(_ => Mock.Of<IPublishEndpoint>());
		services.AddDbContextModule<FirstModule>().AddDbContextModule<SecondModule>();
		services.AddScoped<DomainEventFlusherSaveChangesInterceptor>();
		services.AddDbContext<UnifiedDbContext>((provider, options) => options
			.UseNpgsql(_lease.Slot.PostgresConnectionString)
			.AddInterceptors(provider.GetRequiredService<DomainEventFlusherSaveChangesInterceptor>()));
		services.AddUnitOfWork<UnifiedDbContext>();
		_provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
		_scope = _provider.CreateAsyncScope();
		await Context.Database.ExecuteSqlRawAsync(Context.Database.GenerateCreateScript(), CancellationToken);
	}

	[Fact]
	public async Task ModuleEntities_ShareOneContextAndCommitTogether()
	{
		var first = new FirstEntity();
		var second = new SecondEntity();
		await UnitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
		{
			await UnitOfWork.AddAsync(first, CancellationToken);
			await UnitOfWork.AddAsync(second, CancellationToken);
			Context.Set<FirstEntity>().Local.Should().ContainSingle().Which.Should().BeSameAs(first);
			Context.Set<SecondEntity>().Local.Should().ContainSingle().Which.Should().BeSameAs(second);
			await UnitOfWork.SaveChangesAsync(CancellationToken);
			(await CountPersistedAsync()).Should().Be((0, 0));
		}, CancellationToken);

		(await CountPersistedAsync()).Should().Be((1, 1));
		first.CreatedCalls.Should().Be(1);
		second.CreatedCalls.Should().Be(1);
	}

	[Fact]
	public async Task FailureAfterFirstSave_RollsBackBothModules()
	{
		await Assert.ThrowsAsync<DbUpdateException>(() => UnitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
		{
			await UnitOfWork.AddAsync(new FirstEntity(), CancellationToken);
			await UnitOfWork.SaveChangesAsync(CancellationToken);
			await UnitOfWork.AddAsync(new SecondEntity { Value = null! }, CancellationToken);
			await UnitOfWork.SaveChangesAsync(CancellationToken);
		}, CancellationToken));

		(await CountPersistedAsync()).Should().Be((0, 0));
		UnitOfWork.HasActiveTransaction.Should().BeFalse();
	}

	[Fact]
	public async Task BeforeSave_PreparesEntitiesFromAnotherModuleInTheSameSave()
	{
		var first = new FirstEntity();
		var second = new SecondEntity { Value = "prepared" };
		first.Raise(new PrepareEvent());
		_handle = async (notification, ct) =>
		{
			if (notification is PrepareEvent) await UnitOfWork.AddAsync(second, ct);
		};
		await UnitOfWork.AddAsync(first, CancellationToken);

		await UnitOfWork.SaveChangesAsync(CancellationToken);

		(await CountPersistedAsync()).Should().Be((1, 1));
		first.CreatedCalls.Should().Be(1);
		second.CreatedCalls.Should().Be(1);
	}

	[Fact]
	public async Task OrdinaryEvent_WritesAnotherModuleBeforeCommit()
	{
		_handle = async (notification, ct) =>
		{
			if (notification is CreatedEvent) await UnitOfWork.AddAsync(new SecondEntity(), ct);
		};
		await Services.GetRequiredService<IApplicationTransaction>().WithSettings(new TransactionalAttribute())
			.ExecuteAsync(async (_, ct) =>
			{
				await UnitOfWork.AddAsync(new FirstEntity(), ct);
				await UnitOfWork.SaveChangesAsync(ct);
			}, CancellationToken);

		(await CountPersistedAsync()).Should().Be((1, 1));
	}

	[Fact]
	public async Task EventFailure_RollsBackAlreadySavedEntities()
	{
		_handle = (notification, _) => notification is CreatedEvent
			? Task.FromException(new InvalidOperationException("Event handler failed"))
			: Task.CompletedTask;
		await Assert.ThrowsAsync<InvalidOperationException>(() => Services.GetRequiredService<IApplicationTransaction>()
			.WithSettings(new TransactionalAttribute()).ExecuteAsync(async (ctx, ct) =>
			{
				await ctx.UnitOfWork.AddAsync(new FirstEntity(), ct);
				await ctx.UnitOfWork.AddAsync(new SecondEntity(), ct);
				await ctx.UnitOfWork.SaveChangesAsync(ct);
			}, CancellationToken));

		(await CountPersistedAsync()).Should().Be((0, 0));
	}

	[Fact]
	public void ModelCache_SeparatesDifferentModuleCombinations()
	{
		var options = new DbContextOptionsBuilder<UnifiedDbContext>()
			.UseNpgsql(_lease.Slot.PostgresConnectionString).Options;
		using var first = new UnifiedDbContext(options, [new FirstModule()]);
		using var second = new UnifiedDbContext(options, [new SecondModule()]);
		using var combined = new UnifiedDbContext(options, [new FirstModule(), new SecondModule()]);

		first.Model.FindEntityType(typeof(FirstEntity)).Should().NotBeNull();
		first.Model.FindEntityType(typeof(SecondEntity)).Should().BeNull();
		second.Model.FindEntityType(typeof(FirstEntity)).Should().BeNull();
		second.Model.FindEntityType(typeof(SecondEntity)).Should().NotBeNull();
		combined.Model.FindEntityType(typeof(FirstEntity)).Should().NotBeNull();
		combined.Model.FindEntityType(typeof(SecondEntity)).Should().NotBeNull();
	}

	private async Task<(int First, int Second)> CountPersistedAsync()
	{
		await using var scope = _provider.CreateAsyncScope();
		var context = scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();
		return (await context.Set<FirstEntity>().CountAsync(CancellationToken),
			await context.Set<SecondEntity>().CountAsync(CancellationToken));
	}

	public async ValueTask DisposeAsync()
	{
		try
		{
			await Context.Database.ExecuteSqlRawAsync("DROP SCHEMA unified_tests CASCADE", CancellationToken);
			await _scope.DisposeAsync();
			await _provider.DisposeAsync();
		}
		finally { await _lease.DisposeAsync(); }
	}

	private sealed class FirstModule : IDbContextModule
	{
		public void Configure(ModelBuilder builder)
		{
			builder.Entity<FirstEntity>().ToTable("first", "unified_tests").HasKey(entity => entity.Id);
			builder.Entity<FirstEntity>().Ignore(entity => entity.CreatedCalls);
		}
	}

	private sealed class SecondModule : IDbContextModule
	{
		public void Configure(ModelBuilder builder)
		{
			builder.Entity<SecondEntity>().ToTable("second", "unified_tests").HasKey(entity => entity.Id);
			builder.Entity<SecondEntity>().Property(entity => entity.Value).IsRequired();
			builder.Entity<SecondEntity>().Ignore(entity => entity.CreatedCalls);
		}
	}

	private sealed class FirstEntity : Entity<FirstEntity, Guid>
	{
		public Guid Id { get; private set; } = Guid.NewGuid();
		public int CreatedCalls { get; private set; }
		public override Guid GetId() => Id;
		public void Raise(IDomainEvent item) => AddDomainEvent(item);
		public override void OnCreated() { CreatedCalls++; AddDomainEvent(new CreatedEvent()); }
	}

	private sealed class SecondEntity : Entity<SecondEntity, Guid>
	{
		public Guid Id { get; private set; } = Guid.NewGuid();
		public string Value { get; set; } = "second";
		public int CreatedCalls { get; private set; }
		public override Guid GetId() => Id;
		public override void OnCreated() => CreatedCalls++;
	}

	private sealed record PrepareEvent : IBeforeSaveDomainEvent;
	private sealed record CreatedEvent : IDomainEvent;
}
