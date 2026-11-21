using Tests.TestContainers.Combined;

namespace Tests;

public sealed class ReadScenarioFixture<TScenario>(CombinedContainerFixture environment) : IAsyncLifetime
	where TScenario : ReadScenario, new()
{
	public TScenario Scenario { get; } = new() { Environment = environment };

	public ValueTask InitializeAsync() => Scenario.InitializeAsync();

	public ValueTask DisposeAsync() => Scenario.DisposeAsync();
}
