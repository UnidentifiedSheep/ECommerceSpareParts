using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
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
