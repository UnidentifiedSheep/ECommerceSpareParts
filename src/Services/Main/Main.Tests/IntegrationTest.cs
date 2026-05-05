using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;

namespace Tests;

[Collection("Combined collection")]
public abstract class IntegrationTest(CombinedContainerFixture fixture) : TestBase
{
    private IServiceProvider _sp = null!;
    protected override IServiceProvider Sp => _sp;

    private IServiceScope _scope = null!;
    protected override IServiceScope Scope => _scope;

    protected DContext Context { get; private set; } = null!;
    protected IMediator Mediator { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        _sp = await new ServiceProviderForTests().Build(
            fixture.PostgresConnectionString,
            fixture.RedisConnectionString);
        
        _scope = Sp.CreateScope();
        
        Context = Scope.ServiceProvider.GetRequiredService<DContext>();
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        
        await InitializeBasicContexts();
    }

    public override async Task DisposeAsync()
    {
        await ResetDb();
        Scope.Dispose();
    }
    
    protected Task ResetDb() => Context.ClearDatabase();
}