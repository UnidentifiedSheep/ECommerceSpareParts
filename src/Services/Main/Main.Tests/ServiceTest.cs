using Locan.Hosting;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests;

public abstract class ServiceTest : IAsyncLifetime
{
	private static readonly Lazy<Task<IServiceProvider>> SharedProvider = new(CreateProviderAsync);

	private IServiceScope _scope = null!;

	protected IServiceProvider Services => _scope.ServiceProvider;

	protected IMediator Mediator => Services.GetRequiredService<IMediator>();

	protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	public async ValueTask InitializeAsync()
		=> _scope = (await SharedProvider.Value).CreateScope();

	public ValueTask DisposeAsync()
	{
		_scope.Dispose();
		return ValueTask.CompletedTask;
	}

	private static async Task<IServiceProvider> CreateProviderAsync()
	{
		var provider = new ServiceProviderBuilder().Build(
			new ServiceProviderArguments
			{
				PgsqlConnectionString = "Host=localhost;Port=1;Database=unused;Username=unused;Password=unused",
				CacheConnectionString = "localhost:1"
			});
		await provider.GetServices<IHostedService>()
			.OfType<LocalizerInitializationHostedService>()
			.Single()
			.StartAsync(CancellationToken.None);
		return provider;
	}
}
