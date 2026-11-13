using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;

namespace Tests;

public abstract class IntegrationTest(CombinedContainerFixture fixture)
	: IntegrationTestBase<ServiceProviderBuilder, ServiceProviderArguments, UnifiedDbContext>
{
	private TestEnvironmentLease _environmentLease = null!;
	protected IMediator Mediator { get; private set; } = null!;

	// Saves made directly through the DbContext outside the event pipeline buffer their domain events;
	// call this after such setup so the events are not dispatched by the next handler under test.
	protected void DiscardPendingDomainEvents()
		=> Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();

	public override async ValueTask InitializeAsync()
	{
		_environmentLease = await fixture.AcquireAsync();
		InitializeServiceProvider(
			new ServiceProviderArguments
			{
				PgsqlConnectionString = _environmentLease.Slot.PostgresConnectionString,
				CacheConnectionString = _environmentLease.Slot.RedisConnectionString
			});
		Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();

		await InitializeDatabaseAsync();
		await LoadLocales();
		await InitializeBasicContexts();
	}

	protected override async Task InitializeBasicContexts()
	{
		var unitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
		await unitOfWork.ExecuteWithTransaction(
			new TransactionalAttribute(),
			() => base.InitializeBasicContexts());

		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
	}

	public override async ValueTask DisposeAsync()
	{
		await using var lease = _environmentLease;
		using var scope = Scope;
		await ResetDataStoresAsync();
	}
}
