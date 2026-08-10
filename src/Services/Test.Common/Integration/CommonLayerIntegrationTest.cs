using System.Reflection;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Events;
using Attributes;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Localization.Domain.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.Persistence.Context;
using Tests.TestContainers.Combined;

namespace Tests.Integration;

/// <summary>
/// Base class exclusively for common-layer integration tests declared in
/// Test.Common. Service test projects must use their own integration-test
/// base and DbContext and must not inherit from this class.
/// </summary>
[Collection("Combined collection")]
public abstract class CommonLayerIntegrationTest : TestBase
{
    private readonly CombinedContainerFixture _fixture;
    private IServiceScope _scope = null!;
    private IServiceProvider _serviceProvider = null!;

    internal CommonLayerIntegrationTest(CombinedContainerFixture fixture)
    {
        _fixture = fixture;
    }

    protected override IServiceProvider Sp => _serviceProvider;
    protected override IServiceScope Scope => _scope;

    private protected DContext Context { get; private set; } = null!;
    private protected IMediator Mediator { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        _serviceProvider = new ServiceProviderBuilder().Build(
            new ServiceProviderArguments
            {
                PgsqlConnectionString = _fixture.PostgresConnectionString
            });
        _scope = Sp.CreateScope();

        Context = Scope.ServiceProvider.GetRequiredService<DContext>();
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();

        await Context.Database.EnsureCreatedAsync();
        await LoadLocales();
        await InitializeBasicContexts();
    }

    protected override async Task InitializeBasicContexts()
    {
        var unitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.ExecuteWithTransaction(
            new TransactionalAttribute(),
            () => base.InitializeBasicContexts());

        Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
    }

    public override async Task DisposeAsync()
    {
        await Context.ClearDatabase();
        Scope.Dispose();
    }

    private async Task LoadLocales()
    {
        var containers = Sp.GetRequiredService<IEnumerable<ILocalizerContainer>>();
        var path = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var loader = new JsonLocalizerContainerLoader(path);
        await loader.LoadAsync(containers);
    }
}
