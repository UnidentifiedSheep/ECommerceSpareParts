using System.Reflection;
using Abstractions.Interfaces.Services;
using Api.Common.Extensions;
using Attributes;
using Localization.Domain.Extensions;
using Main.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using StackExchange.Redis;
using Test.Common.Abstractions.Test;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;

namespace Tests;

[Collection("Combined collection")]
public abstract class IntegrationTest(CombinedContainerFixture fixture)
    : IntegrationTestBase<ServiceProviderBuilder, ServiceProviderArguments, DContext>
{
    protected IMediator Mediator { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        InitializeServiceProvider(new ServiceProviderArguments
        {
            PgsqlConnectionString = fixture.PostgresConnectionString,
            CacheConnectionString = fixture.RedisConnectionString
        });
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();

        await ResetCache();
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
        await ResetCache();
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

    protected async Task ResetCache()
    {
        var multiplexer = Scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var database = multiplexer.GetDatabase();

        foreach (var endpoint in multiplexer.GetEndPoints())
        {
            var server = multiplexer.GetServer(endpoint);
            var keys = server.Keys(database.Database).ToArray();

            if (keys.Length == 0)
                continue;

            await database.KeyDeleteAsync(keys);
        }
    }
}