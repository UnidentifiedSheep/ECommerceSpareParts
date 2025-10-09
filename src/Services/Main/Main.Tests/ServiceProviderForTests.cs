using Api.Common;
using Core.Interfaces;
using Core.Models;
using Mail;
using Main.Application.Configs;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Contexts;
using Security;
using Serilog;
using Tests.MockData;
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
            .AddSecurityLayer(passwordRules)
            .AddMailLayer()
            .AddCommonLayer();
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
        var priceSetup = scope.ServiceProvider.GetRequiredService<IPriceSetup>();

        await context.AddMockCurrencies();
        await priceSetup.SetupAsync();
    }
}