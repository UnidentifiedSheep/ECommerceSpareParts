using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Events;
using Attributes;
using Locan.Hosting;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.Persistence.Context;
using Tests.TestContainers.Combined;

namespace Tests.Integration;

/// <summary>
///     Base class exclusively for common-layer integration tests declared in
///     Test.Common. Service test projects must use their own integration-test
///     base and DbContext and must not inherit from this class.
/// </summary>
public abstract class CommonLayerIntegrationTest : TestBase
{
	private readonly CombinedContainerFixture _fixture;

	private IServiceScope _scope = null!;

	private IServiceProvider _serviceProvider = null!;

	internal CommonLayerIntegrationTest(CombinedContainerFixture fixture)
	{
		_fixture = fixture;
	}

	protected override IServiceProvider Sp => _serviceProvider;

	protected override IServiceScope Scope => _scope;

	protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private protected DContext Context { get; private set; } = null!;

	private protected IMediator Mediator { get; private set; } = null!;

	private TestEnvironmentLease _environmentLease = null!;

	public override async ValueTask InitializeAsync()
	{
		_environmentLease = await _fixture.AcquireAsync();

		_serviceProvider = new ServiceProviderBuilder().Build(
			new ServiceProviderArguments
			{
				PgsqlConnectionString = _environmentLease.Slot.PostgresConnectionString
			});
		_scope = Sp.CreateScope();

		Context = Scope.ServiceProvider.GetRequiredService<DContext>();
		Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();

		await Context.Database.EnsureCreatedAsync();
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
		await Context.ClearDatabase();
		await _environmentLease.DisposeAsync();
		Scope.Dispose();
	}

	private async Task LoadLocales()
	{
		var task = Sp
			.GetServices<IHostedService>()
			.OfType<LocalizerInitializationHostedService>()
			.Single();
		await task.StartAsync(CancellationToken.None);
	}
}
