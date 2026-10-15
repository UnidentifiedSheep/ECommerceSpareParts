using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces.Product;
using Search.Entities;
using Search.Persistence.Interfaces;
using StackExchange.Redis;
using Tests.Abstractions.Test;
using Tests.TestContainers.Search;

namespace Search.Tests;

public abstract class IntegrationTest(SearchContainerFixture fixture) : TestBase
{
	private SearchTestEnvironmentLease? _lease;
	private IServiceProvider? _provider;
	private IServiceScope? _scope;
	private bool _disposed;

	protected override IServiceProvider Sp => _provider!;
	protected override IServiceScope Scope => _scope!;
	protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;
	protected IOpenSearchClient Client { get; private set; } = null!;
	protected IProductRepository Products { get; private set; } = null!;
	protected OpenSearchIndexOptions Indexes { get; private set; } = null!;
	protected IMediator Mediator { get; private set; } = null!;

	public override async ValueTask InitializeAsync()
	{
		_lease = await fixture.AcquireAsync(CancellationToken);
		_provider = new ServiceProviderBuilder().Build(new ServiceProviderArguments
		{
			OpenSearchEndpoint = _lease.Slot.OpenSearch.Endpoint,
			IndexPrefix = _lease.Slot.OpenSearch.Prefix,
			RedisConnectionString = _lease.Slot.Redis.ConnectionString
		});
		_scope = Sp.CreateScope();
		Client = Scope.ServiceProvider.GetRequiredService<IOpenSearchClient>();
		Products = Scope.ServiceProvider.GetRequiredService<IProductRepository>();
		Indexes = Scope.ServiceProvider.GetRequiredService<IOptions<OpenSearchOptions>>().Value.IndexOptions;
		Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
		await ResetDataStoresAsync(CancellationToken);
		await Task.WhenAll(
			Scope.ServiceProvider.GetRequiredService<IIndexInitializer<Product>>().LazyInitialize(CancellationToken),
			Scope.ServiceProvider.GetRequiredService<IIndexInitializer<Producer>>().LazyInitialize(CancellationToken),
			Scope.ServiceProvider.GetRequiredService<IIndexInitializer<CatalogueCandidate>>().LazyInitialize(CancellationToken));
		await InitializeBasicContexts();
	}

	public override async ValueTask DisposeAsync()
	{
		if (_disposed) return;
		_disposed = true;
		try
		{
			if (_scope is not null)
				await ResetDataStoresAsync(CancellationToken);
		}
		finally
		{
			try
			{
				_scope?.Dispose();
				if (_provider is IAsyncDisposable disposable)
					await disposable.DisposeAsync();
			}
			finally
			{
				if (_lease is not null)
					await _lease.DisposeAsync();
			}
		}
	}

	protected async Task RefreshAsync()
	{
		var response = await Client.Indices.RefreshAsync(IndexPattern, ct: CancellationToken);
		EnsureSucceeded(response);
	}

	private string IndexPattern => _lease!.Slot.OpenSearch.Prefix + "*";

	private async Task ResetDataStoresAsync(CancellationToken cancellationToken = default)
	{
		var response = await Client.Indices.DeleteAsync(IndexPattern, request => request.IgnoreUnavailable(), cancellationToken);
		EnsureSucceeded(response);
		var multiplexer = Scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
		var database = multiplexer.GetDatabase();
		foreach (var endpoint in multiplexer.GetEndPoints())
		{
			var keys = multiplexer.GetServer(endpoint).Keys(database.Database).ToArray();
			if (keys.Length > 0) await database.KeyDeleteAsync(keys);
		}
	}

	private static void EnsureSucceeded(IResponse response)
	{
		if (!response.IsValid)
			throw new InvalidOperationException(response.DebugInformation);
	}
}
