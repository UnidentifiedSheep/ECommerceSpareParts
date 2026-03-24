using System.Reflection;
using Abstractions.Interfaces.Currency;
using Analytics.Application;
using Analytics.Application.Configs.Mapster;
using Analytics.Integration.Tests.MockData;
using Microsoft.Extensions.DependencyInjection;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Localization.Domain.Extensions;
using MassTransit;
using Persistence.Extensions;
using Redis;
using Test.Common.Stubs;
using MsServiceProvider = Microsoft.Extensions.DependencyInjection.ServiceProvider;
namespace Analytics.Integration.Tests;

public static class ServiceProviderForTests
{
    private static bool _isConfiguredBefore;
    private static MsServiceProvider? _serviceProvider;

    public static IServiceProvider Build(string postgresConnectionString, string redisConnectionString)
    {
        if (_isConfiguredBefore)
        {
            var scope = _serviceProvider!.CreateScope();
            return scope.ServiceProvider;
        }
        
        var locales = new[] {"ru-RU", "en-EN"};
        string localesPath = Assembly.GetExecutingAssembly().Location;
        localesPath = Path.Combine(Path.GetDirectoryName(localesPath)!, "Localization");
        
        IServiceCollection services = new ServiceCollection();
        
        services.AddPersistenceLayer(postgresConnectionString)
            .AddApplicationLayer()
            .AddLocalization(locales)
            .AddCacheLayer(redisConnectionString);
        
        services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
        MapsterConfig.Configure();
        
        var serviceProvider = services.BuildServiceProvider();
        _serviceProvider = serviceProvider;
        
        SeedDb(serviceProvider).Wait();
        SetupPrice(_serviceProvider).Wait();
        serviceProvider.LoadLocalesFromJson(localesPath).Wait();

        _isConfiguredBefore = true;
        return serviceProvider;
    }
    
    private static async Task SeedDb(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.SeedAsync<DContext>();
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