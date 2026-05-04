using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Abstractions.Models;
using Api.Common;
using Mail;
using Main.Application.Configs;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Extensions;
using Security;
using Serilog;
using Test.Common.Extensions;
using Test.Common.Stubs;
using Test.Common.TestContexts;
using Tests.TestContexts.Basic;
using ApplicationServiceProvider = Main.Application.ServiceProvider;

namespace Tests;

public class ServiceProviderForTests
{
    public async Task<IServiceProvider> Build(string postgresConnectionString, string redisConnectionString)
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
        
        services.RemoveAll<IUserContext>();
        services.AddScoped<IUserContext, UserContextMock>();
        
        services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
        SortByConfig.Configure();


        var serviceProvider = services.BuildServiceProvider();

        await SeedDb(serviceProvider);
        await SetupPrice(serviceProvider);
        return serviceProvider;
    }

    private static void RegisterGlobalBasicContexts()
    {
        TestBase.RegisterGlobalBasicContext<UserContextTestContext>();
        TestBase.RegisterGlobalBasicContext<LocalizedTestContext>();
    }

    private static async Task SeedDb(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.SeedAsync<DContext>();
    }

    private static async Task SetupPrice(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var currencyConverterSetup = scope.ServiceProvider.GetRequiredService<ICurrencyConverterSetup>();
        await currencyConverterSetup.InitializeAsync(null);
    }
}