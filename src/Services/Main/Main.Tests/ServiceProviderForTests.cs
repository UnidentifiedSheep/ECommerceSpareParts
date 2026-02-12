using Abstractions.Interfaces.Currency;
using Abstractions.Models;
using Api.Common;
using Mail;
using Main.Application.Configs;
using Main.Cache;
using Microsoft.Extensions.DependencyInjection;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Security;
using Serilog;
using Tests.MockData;
using Tests.Stubs;
using ApplicationServiceProvider = Main.Application.ServiceProvider;
using CacheServiceProvider = Redis.ServiceProvider;
using ServiceProvider = Microsoft.Extensions.DependencyInjection.ServiceProvider;

namespace Tests;

public static class ServiceProviderForTests
{
    private static bool _isConfiguredBefore;
    private static ServiceProvider? _serviceProvider;

    public static IServiceProvider Build(string postgresConnectionString, string redisConnectionString)
    {
        if (_isConfiguredBefore)
        {
            var scope = _serviceProvider!.CreateScope();
            return scope.ServiceProvider;
        }

        var services = new ServiceCollection();

        services.AddLogging();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        ApplicationServiceProvider.AddApplicationLayer(services)
            .AddPersistenceLayer(postgresConnectionString);
        var passwordRules = new PasswordRules
        {
            RequireDigit = false,
            RequireUppercase = false
        };
        CacheServiceProvider.AddCacheLayer(services, redisConnectionString)
            .AddSecurityLayer("some secret", null, passwordRules)
            .AddMailLayer()
            .AddCommonLayer()
            .AddAppCacheLayer();

        services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
        MapsterConfig.Configure();
        SortByConfig.Configure();


        var serviceProvider = services.BuildServiceProvider();
        _isConfiguredBefore = true;
        _serviceProvider = serviceProvider;
        SetupPrice(_serviceProvider).Wait();
        return serviceProvider;
    }

    private static async Task SetupPrice(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DContext>();
        var currencyConverterSetup = scope.ServiceProvider.GetRequiredService<ICurrencyConverterSetup>();

        await context.AddMockCurrencies();
        await currencyConverterSetup.InitializeAsync();
    }
}