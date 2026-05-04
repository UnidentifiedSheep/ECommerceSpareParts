using Abstractions.Interfaces.Tests;
using Bogus;
using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;

namespace Tests;

[Collection("Combined collection")]
public abstract class TestBase(CombinedContainerFixture fixture) : IAsyncLifetime
{
    private static readonly HashSet<Type> GlobalBasicContexts = [];
    private readonly HashSet<Type> _basicContexts = []; 
    protected IServiceProvider Sp { get; private set; } = null!;
    protected IServiceScope Scope { get; private set; } = null!;
    
    protected DContext Context { get; private set; } = null!;
    protected IMediator Mediator { get; private set; } = null!;
    protected readonly Faker Faker = new();
    

    public virtual async Task InitializeAsync()
    {
        Sp = await new ServiceProviderForTests().Build(
            fixture.PostgresConnectionString,
            fixture.RedisConnectionString);
        
        Scope = Sp.CreateScope();
        
        Context = Scope.ServiceProvider.GetRequiredService<DContext>();
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        
        await InitializeBasicContexts();
    }

    public virtual async Task DisposeAsync()
    {
        await ResetDb();
        Scope.Dispose();
    }
    
    protected Task ResetDb() => Context.ClearDatabase();
    
    protected void RegisterBasicContext<TContext>() where TContext : class, ITestContext
    {
        _basicContexts.Add(typeof(TContext));
    }

    protected void RemoveBasicContext<TContext>() where TContext : class, ITestContext
    {
        _basicContexts.Remove(typeof(TContext));
    }
    
    public static void RegisterGlobalBasicContext<TContext>() where TContext : class, ITestContext
    {
        GlobalBasicContexts.Add(typeof(TContext));
    }

    public static void RemoveGlobalBasicContext<TContext>() where TContext : class, ITestContext
    {
        GlobalBasicContexts.Remove(typeof(TContext));
    }

    private async Task InitializeBasicContexts()
    {
        var merged = GlobalBasicContexts.ToHashSet();
        merged.UnionWith(_basicContexts);
        foreach (var context in merged)
            await ((ITestContext)Scope.ServiceProvider.GetService(context)!).InitializeAsync();
    }
}

public abstract class TestBase<TTestContext>(CombinedContainerFixture fixture) : TestBase(fixture) 
    where TTestContext : class, ITestContext
{
    protected TTestContext TestContext { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        RegisterBasicContext<TTestContext>();
        await base.InitializeAsync();
    }
}