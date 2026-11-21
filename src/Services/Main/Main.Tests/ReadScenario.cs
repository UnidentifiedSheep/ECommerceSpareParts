using Application.Common.Interfaces.Persistence;
using Attributes;
using Locan.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Persistence.Context;
using Persistence.Interfaces;
using StackExchange.Redis;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using ITestContext = Tests.Interfaces.ITestContext;

namespace Tests;

public abstract class ReadScenario : TestBase
{
	private const string ReadRole = "read_scenario";

	private TestEnvironmentLease _lease = null!;

	private IServiceProvider _readProvider = null!;

	private IServiceScope _scope = null!;

	private IServiceProvider _sp = null!;

	internal CombinedContainerFixture Environment { get; set; } = null!;

	protected override IServiceProvider Sp => _sp;

	protected override IServiceScope Scope => _scope;

	protected UnifiedDbContext Context { get; private set; } = null!;

	protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	public T Get<T>() where T : class, ITestContext => GetContext<T>();

	protected abstract IEnumerable<object> Seed();

	public override async ValueTask InitializeAsync()
	{
		_lease = await Environment.AcquireReadScenarioAsync(CancellationToken);
		var arguments = new ServiceProviderArguments
		{
			PgsqlConnectionString = _lease.Slot.PostgresConnectionString,
			CacheConnectionString = _lease.Slot.RedisConnectionString
		};
		_sp = new ServiceProviderBuilder().Build(arguments);
		_scope = _sp.CreateScope();
		Context = _scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();

		await Context.Database.MigrateAsync(CancellationToken);
		await Context.ClearDatabase(CancellationToken);
		await ClearCacheAsync();
		await RunSeedsAsync();
		await LoadLocalesAsync(_sp);

		await _scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
			.ExecuteWithTransaction(new TransactionalAttribute(), InitializeBasicContexts);
		DiscardPendingDomainEvents();

		Context.AddRange(Seed());
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();
		await GrantReadAccessAsync();

		_readProvider = new ServiceProviderBuilder().Build(
			arguments with { PgsqlConnectionString = ToReadOnly(arguments.PgsqlConnectionString) });
		await LoadLocalesAsync(_readProvider);
	}

	internal IServiceScope CreateReadScope() => _readProvider.CreateScope();

	public override async ValueTask DisposeAsync()
	{
		_scope.Dispose();
		await DisposeProviderAsync(_readProvider);
		await DisposeProviderAsync(_sp);
		await _lease.DisposeAsync();
	}

	private async Task RunSeedsAsync()
	{
		await using var scope = _sp.CreateAsyncScope();
		var context = scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();
		foreach (var seed in scope.ServiceProvider.GetServices<ISeed<UnifiedDbContext>>()
					.OrderBy(seed => seed.ExecutionOrder))
			await seed.SeedAsync(context);
	}

	private async Task ClearCacheAsync()
	{
		var multiplexer = _scope.ServiceProvider.GetService<IConnectionMultiplexer>();
		if (multiplexer is null)
			return;

		var database = multiplexer.GetDatabase();
		foreach (var endpoint in multiplexer.GetEndPoints())
		{
			var keys = multiplexer.GetServer(endpoint).Keys(database.Database).ToArray();
			if (keys.Length > 0)
				await database.KeyDeleteAsync(keys);
		}
	}

	private static Task LoadLocalesAsync(IServiceProvider provider)
		=> provider.GetServices<IHostedService>()
			.OfType<LocalizerInitializationHostedService>()
			.Single()
			.StartAsync(CancellationToken.None);

	private Task GrantReadAccessAsync()
		=> Context.Database.ExecuteSqlRawAsync(
			$$"""
			DO $$
			BEGIN
			    CREATE ROLE {{ReadRole}} LOGIN PASSWORD '{{ReadRole}}';
			EXCEPTION WHEN duplicate_object OR unique_violation THEN NULL;
			END $$;
			ALTER ROLE {{ReadRole}} SET default_transaction_read_only = on;
			DO $$
			DECLARE
			    schema_name text;
			BEGIN
			    FOR schema_name IN
			        SELECT nspname
			        FROM pg_namespace
			        WHERE nspname NOT LIKE 'pg\_%' AND nspname <> 'information_schema'
			    LOOP
			        EXECUTE format('GRANT USAGE ON SCHEMA %I TO {{ReadRole}}', schema_name);
			        EXECUTE format('GRANT SELECT ON ALL TABLES IN SCHEMA %I TO {{ReadRole}}', schema_name);
			        EXECUTE format('GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA %I TO {{ReadRole}}', schema_name);
			    END LOOP;
			END $$;
			""",
			CancellationToken);

	private static string ToReadOnly(string connectionString)
		=> new NpgsqlConnectionStringBuilder(connectionString)
		{
			Username = ReadRole,
			Password = ReadRole
		}.ConnectionString;

	private static async ValueTask DisposeProviderAsync(IServiceProvider? provider)
	{
		switch (provider)
		{
			case IAsyncDisposable asyncDisposable:
				await asyncDisposable.DisposeAsync();
				break;
			case IDisposable disposable:
				disposable.Dispose();
				break;
		}
	}
}
