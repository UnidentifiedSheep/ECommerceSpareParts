using System.Reflection;
using Abstractions.Interfaces.Persistence;
using Api.Common.Extensions;
using Application.Common.Interfaces.Events;
using Attributes;
using Localization.Domain.Extensions;
using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;

namespace Tests;

[Collection("Combined collection")]
public abstract class IntegrationTest(CombinedContainerFixture fixture)
    : IntegrationTestBase<ServiceProviderBuilder, ServiceProviderArguments, DContext>
{
    protected IMediator Mediator { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        InitializeServiceProvider(
            new ServiceProviderArguments
            {
                PgsqlConnectionString = fixture.PostgresConnectionString,
                CacheConnectionString = fixture.RedisConnectionString
            });
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();

        await ResetDataStoresAsync();
        await SeedDb();
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
        await ResetDataStoresAsync();
        Scope.Dispose();
    }

    private async Task SeedDb()
    {
        using var scope = Sp.CreateScope();
        await scope.SeedAsync<DContext>();
    }

}
