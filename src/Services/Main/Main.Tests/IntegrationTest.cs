using System.Reflection;
using Abstractions.Interfaces.Services;
using Api.Common.Extensions;
using Attributes;
using Localization.Domain.Extensions;
using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;

namespace Tests;

[Collection("Combined collection")]
public abstract class IntegrationTest(CombinedContainerFixture fixture) : TestBase
{
    private IServiceScope _scope = null!;
    private IServiceProvider _sp = null!;
    protected override IServiceProvider Sp => _sp;
    protected override IServiceScope Scope => _scope;

    protected DContext Context { get; private set; } = null!;
    protected IMediator Mediator { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        _sp = new ServiceProviderForTests().Build(
            fixture.PostgresConnectionString,
            fixture.RedisConnectionString);

        _scope = Sp.CreateScope();

        Context = Scope.ServiceProvider.GetRequiredService<DContext>();
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();

        await SeedDb();
        await LoadLocales();
        await InitializeBasicContexts();
    }

    protected override async Task InitializeBasicContexts()
    {
        var unitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), () => base.InitializeBasicContexts());
    }

    public override async Task DisposeAsync()
    {
        await ResetDb();
        Scope.Dispose();
    }

    private async Task LoadLocales()
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        await Sp.LoadLocalesFromJson(localesPath);
    }

    private async Task SeedDb()
    {
        using var scope = Sp.CreateScope();
        await scope.SeedAsync<DContext>();
    }

    protected Task ResetDb()
    {
        return Context.ClearDatabase();
    }
}