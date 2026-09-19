using Locan.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Tests.Extensions;
using Tests.Interfaces.ServiceProvider;

namespace Tests.Abstractions.Test;

public abstract class IntegrationTestBase<TSp, TArgs, TContext> : TestBase
	where TSp : IServiceProviderBuilder<TArgs>, new()
	where TArgs : IServiceProviderArgument
	where TContext : DbContext
{
	private IServiceScope _scope = null!;

	private IServiceProvider _sp = null!;

	protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	protected override IServiceProvider Sp => _sp;

	protected override IServiceScope Scope => _scope;

	public TContext Context { get; private set; } = null!;

	protected void InitializeServiceProvider(TArgs args)
	{
		_sp = new TSp().Build(args);
		_scope = Sp.CreateScope();
		Context = _scope.ServiceProvider.GetRequiredService<TContext>();
	}

	protected async Task LoadLocales()
	{
		var task = Sp.GetServices<IHostedService>().OfType<LocalizerInitializationHostedService>().Single();
		await task.StartAsync(CancellationToken.None);
	}

	protected async Task ResetDataStoresAsync(CancellationToken cancellationToken = default)
	{
		await Context.ClearDatabase(cancellationToken);

		var multiplexer = Scope.ServiceProvider.GetService<IConnectionMultiplexer>();
		if (multiplexer is null)
			return;

		var database = multiplexer.GetDatabase();
		foreach (var endpoint in multiplexer.GetEndPoints())
		{
			var server = multiplexer.GetServer(endpoint);
			var keys = server.Keys(database.Database).ToArray();

			if (keys.Length == 0)
				continue;

			await database.KeyDeleteAsync(keys);
		}
	}
}
