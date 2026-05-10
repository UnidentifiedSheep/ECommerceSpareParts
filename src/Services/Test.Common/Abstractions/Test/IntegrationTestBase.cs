using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Interfaces.ServiceProvider;

namespace Test.Common.Abstractions.Test;

public abstract class IntegrationTestBase<TSp, TArgs, TContext> : TestBase
    where TSp : IServiceProviderBuilder<TArgs>, new()
    where TArgs : IServiceProviderArgument
    where TContext : DbContext
{
    private IServiceScope _scope = null!;
    private IServiceProvider _sp = null!;
    protected override IServiceProvider Sp => _sp;
    protected override IServiceScope Scope => _scope;

    public TContext Context { get; private set; } = null!;

    protected void InitializeServiceProvider(TArgs args)
    {
        _sp = new TSp().Build(args);
        _scope = Sp.CreateScope();
        Context = _scope.ServiceProvider.GetRequiredService<TContext>();
    }
}