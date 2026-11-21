using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public abstract class ReadScenarioTest<TScenario>(ReadScenarioFixture<TScenario> fixture)
	: IClassFixture<ReadScenarioFixture<TScenario>>, IAsyncLifetime
	where TScenario : ReadScenario, new()
{
	private IServiceScope _scope = null!;

	protected TScenario Scenario { get; } = fixture.Scenario;

	protected IServiceProvider Services => _scope.ServiceProvider;

	protected IMediator Mediator => Services.GetRequiredService<IMediator>();

	protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	public ValueTask InitializeAsync()
	{
		_scope = Scenario.CreateReadScope();
		return ValueTask.CompletedTask;
	}

	public ValueTask DisposeAsync()
	{
		_scope.Dispose();
		return ValueTask.CompletedTask;
	}
}
