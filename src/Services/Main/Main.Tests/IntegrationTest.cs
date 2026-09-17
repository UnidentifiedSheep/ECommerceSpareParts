using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Events;
using Attributes;
using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;

namespace Tests;

public abstract class IntegrationTest(CombinedContainerFixture fixture)
	: IntegrationTestBase<ServiceProviderBuilder, ServiceProviderArguments, DContext>
{
	private TestEnvironmentLease _environmentLease = null!;
	protected IMediator Mediator { get; private set; } = null!;

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

		await ResetDataStoresAsync();
		await SeedDb();
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
		await ResetDataStoresAsync();
		await _environmentLease.DisposeAsync();
		Scope.Dispose();
	}

	private async Task SeedDb()
	{
		using var scope = Sp.CreateScope();
		await scope.SeedAsync<DContext>();
	}
}
