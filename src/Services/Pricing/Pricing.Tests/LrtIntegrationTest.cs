using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Persistence.Contexts;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests;

public abstract class LrtIntegrationTest<TLrt>(CombinedContainerFixture fixture)
	: LrtIntegrationTestBase<TLrt, ServiceProviderBuilder, ServiceProviderArguments, DContext>
	where TLrt : class, ILrtNamedObject
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
	}

	public override async ValueTask DisposeAsync()
	{
		await using var lease = _environmentLease;
		using var scope = Scope;
		await ResetDataStoresAsync();
	}

}
