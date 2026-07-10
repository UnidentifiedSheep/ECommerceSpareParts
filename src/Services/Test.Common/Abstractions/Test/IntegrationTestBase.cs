using System.Reflection;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Localization.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Interfaces.ServiceProvider;

namespace Tests.Abstractions.Test;

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
    
    protected async Task LoadLocales()
    {
        var containers = Sp.GetRequiredService<IEnumerable<ILocalizerContainer>>();
        var path = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var loader = new JsonLocalizerContainerLoader(path);
        await loader.LoadAsync(containers);
    }
}