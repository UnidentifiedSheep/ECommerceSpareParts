using Abstractions.Interfaces.Tests;
using Main.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;

namespace Tests;

[Collection("Combined collection")]
public abstract class TestBase(CombinedContainerFixture fixture) : IAsyncLifetime
{
    protected IServiceProvider Sp = null!;
    protected DContext Context = null!;

    public virtual async Task InitializeAsync()
    {
        Sp = await new ServiceProviderForTests().Build(
            fixture.PostgresConnectionString,
            fixture.RedisConnectionString);
        Context = Sp.GetRequiredService<DContext>();
    }

    public virtual Task DisposeAsync()
    {
        return ResetDb();
    }
    
    protected Task ResetDb() => Context.ClearDatabase();
}

public abstract class TestBase<TTestContext>(CombinedContainerFixture fixture) : TestBase(fixture) 
    where TTestContext : class, ITestContext
{
    protected TTestContext TestContext { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        TestContext = Sp.GetRequiredService<TTestContext>();
        await TestContext.InitializeAsync();
    }
}