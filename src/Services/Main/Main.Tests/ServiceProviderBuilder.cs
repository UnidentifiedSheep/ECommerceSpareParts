using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common;
using Cache;
using Mail;
using Main.Application.Configs;
using Main.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Security;
using Serilog;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Test.Common.Stubs;
using Test.Common.TestContexts;
using Tests.TestContexts;
using ApplicationServiceProvider = Main.Application.ServiceProvider;

namespace Tests;

public class ServiceProviderBuilder : IServiceProviderBuilder<ServiceProviderArguments>
{
    private static bool _staticsConfigured;

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

        ApplicationServiceProvider.AddApplicationLayer(services)
            .AddPersistenceLayer(args.PgsqlConnectionString);
        var passwordRules = new PasswordRules
        {
            RequireDigit = false,
            RequireUppercase = false
        };

        services.AddJsonSigner("some secret")
            .AddCacheLayer(args.CacheConnectionString)
            .AddFullSecurityLayer(passwordRules)
            .AddMailLayer()
            .AddCommonLayer();

        services.RemoveAll<IUserContext>();
        services.AddScoped<IUserContext, UserContextMock>();

        services.AddTransient<IPublishEndpoint, MessageBrokerStub>();

        if (!_staticsConfigured)
        {
            _staticsConfigured = true;
            SortByConfig.Configure();
        }

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }

    private static void RegisterGlobalBasicContexts()
    {
        TestBase.RegisterGlobalBasicContext<LocalizedTestContext>();
        TestBase.RegisterGlobalBasicContext<RolesTestContext>();
        TestBase.RegisterGlobalBasicContext<UserContextTestContext>();
    }
}