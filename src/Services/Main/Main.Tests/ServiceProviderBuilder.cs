using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common;
using Cache;
using Localization.Domain.Extensions;
using Mail;
using Main.Application.Configs;
using Main.Cache;
using Main.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Persistence;
using Security;
using Serilog;
using Test.Common.Abstractions.Test;
using Test.Common.Extensions;
using Test.Common.Interfaces.ServiceProvider;
using Test.Common.Stubs;
using Test.Common.TestContexts;
using Tests.TestContexts;
using ZiggyCreatures.Caching.Fusion.Backplane;
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

        ApplicationServiceProvider
            .AddApplicationLayer(services, null)
            .AddLocalization("ru-RU", "ru-RU", "en-EN")
            .AddPersistenceLayer();
        var passwordRules = new PasswordRules
        {
            RequireDigit = false,
            RequireUppercase = false
        };

        services.AddSingleton(Options.Create(new RedisOptions
        {
            Url = args.CacheConnectionString,
            Password = null
        }));

        var pgsqlBuilder = new NpgsqlConnectionStringBuilder(args.PgsqlConnectionString);

        services.AddSingleton(Options.Create(new DatabaseOptions
        {
            Host = pgsqlBuilder.Host!,
            Database = pgsqlBuilder.Database!,
            Username = pgsqlBuilder.Username!,
            Password = pgsqlBuilder.Password!,
            Port = pgsqlBuilder.Port
        }));

        services.AddJsonSigner("some secret")
            .AddCacheLayer("test")
            .AddApplicationCache()
            .AddFullSecurityLayer(passwordRules)
            .AddMailLayer()
            .AddCommonLayer();

        services.RemoveAll<IUserContext>();
        services.AddScoped<IUserContext, UserContextMock>();

        services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
        services.RemoveAll<IFusionCacheBackplane>();
        services.AddSingleton<IFusionCacheBackplane, FusionCacheBackplaneStub>();

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
