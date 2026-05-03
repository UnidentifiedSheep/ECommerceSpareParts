using Abstractions.Interfaces.Currency;
using Abstractions.Models;
using Api.Common;
using Mail;
using Main.Application.Configs;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using Security;
using Serilog;
using Test.Common.Extensions;
using Test.Common.Stubs;
using Tests.MockData;
using ApplicationServiceProvider = Main.Application.ServiceProvider;

namespace Tests;

public class ServiceProviderForTests
{
    public async Task<IServiceProvider> Build(string postgresConnectionString, string redisConnectionString)
    {
        var services = new ServiceCollection();
        
        services.RegisterTestContexts();

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
            
            services.AddJsonSigner("some secret")
            .AddFullSecurityLayer(passwordRules)
            .AddMailLayer()
            .AddCommonLayer();

        services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
        SortByConfig.Configure();


        var serviceProvider = services.BuildServiceProvider();

        await SeedDb(serviceProvider);
        await SetupPrice(serviceProvider);
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
        await currencyConverterSetup.InitializeAsync(null);
    }
}