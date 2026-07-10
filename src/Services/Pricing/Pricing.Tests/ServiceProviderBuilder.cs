using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common;
using Cache;
using Localization.Domain.Extensions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Persistence;
using Pricing.Persistence;
using Serilog;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.Interfaces.ServiceProvider;
using Tests.Stubs;
using Tests.TestContexts;
using ZiggyCreatures.Caching.Fusion.Backplane;
using ApplicationServiceProvider = Pricing.Application.ServiceProvider;

namespace Pricing.Integration.Tests;

public class ServiceProviderBuilder : IServiceProviderBuilder<ServiceProviderArguments>
{
    public IServiceProvider Build(ServiceProviderArguments args)
    {
        RegisterGlobalBasicContexts();
        var services = new ServiceCollection();

        services.RegisterTestContexts();

        services.AddLogging();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        ApplicationServiceProvider
            .AddApplicationLayer(services, null!)
            .AddLocalization(
                "ru-RU",
                "ru-RU",
                "en-EN")
            .AddPersistenceLayer();

        services.AddSingleton(
            Options.Create(
                new RedisOptions
                {
                    Url = args.CacheConnectionString,
                    Password = null
                }));

        var pgsqlBuilder = new NpgsqlConnectionStringBuilder(args.PgsqlConnectionString);

        services.AddSingleton(
            Options.Create(
                new DatabaseOptions
                {
                    Host = pgsqlBuilder.Host!,
                    Database = pgsqlBuilder.Database!,
                    Username = pgsqlBuilder.Username!,
                    Password = pgsqlBuilder.Password!,
                    Port = pgsqlBuilder.Port
                }));

        services
            .AddCacheLayer("test")
            .AddCommonLayer();

        services.RemoveAll<IUserContext>();
        services.AddScoped<IUserContext, UserContextMock>();
        services.AddSystemOptionsForTests();

        services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
        services.RemoveAll<IFusionCacheBackplane>();
        services.AddSingleton<IFusionCacheBackplane, FusionCacheBackplaneStub>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }

    private static void RegisterGlobalBasicContexts()
    {
        TestBase.RegisterGlobalBasicContext<LocalizedTestContext>();
    }
}
