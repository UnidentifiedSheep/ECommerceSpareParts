using System.Reflection;
using Abstractions;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Application.Common.Interfaces;
using Application.Common.Services.Supplier;
using Cache;
using Carter;
using Contracts.Analytics;
using Contracts.Job;
using Contracts.Settings;
using Integrations.Supplier.DI;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using OpenTelemetry.Metrics;
using Pricing.Api;
using Pricing.Api.Startup;
using Pricing.Application;
using Pricing.Application.Consumers;
using Pricing.Application.Interfaces.Markup;
using Pricing.Cache;
using Pricing.Persistence;
using Pricing.Persistence.Contexts;
using RabbitMq.Extensions;
using Security;
using ZiggyCreatures.Caching.Fusion.Backplane;

var builder = WebApplication.CreateBuilder(args);

var env = builder.AddServiceConfiguration("pricing");

builder.Host.AddLokiLogger(
    builder.Configuration,
    "pricing.api",
    env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions()
    .AddSecretEncryptionOptions();

builder.Services.AddCommonApiInfrastructure();

var uniqQueueName = $"queue-of-pricing-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(Global)));
    x.AddConsumer<BackplaneConsumer>();
    x.AddConsumer<SettingUpdatedConsumer>();

    x.AddEntityFrameworkOutbox<DContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);

        cfg.ReceiveEndpoint(
            uniqQueueName,
            ep =>
            {
                ep.AutoDelete = true;
                ep.Durable = false;
                ep.ConfigureConsumeTopology = false;

                ep.ConfigureConsumer<BackplaneConsumer>(context);
                ep.ConfigureConsumer<SettingUpdatedConsumer>(context);

                ep.ConfigureConsumer<MarkupRangesRefreshRequestedConsumer>(context);

                ep.Bind<BackplaneMessage>();
                ep.Bind<MarkupRangesRefreshRequestedEvent>();

                ep.BindForService<JobStatusUpdatedEvent>(ServicesDefinitions.Pricing)
                    .BindForService<SettingUpdatedEvent>(ServicesDefinitions.Pricing);
            });

        cfg.ReceiveEndpoint(
            "pricing-queue",
            ep =>
            {
                ep.Durable = true;

                ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
                ep.ConfigureConsumer<MarkupAnalyzedConsumer>(context);
            });
    });
});

builder.Services
    .AddEComAuth(builder.Configuration)
    .AddPersistenceLayer()
    .AddApplicationCache()
    .AddFavoriteIntegration<FavoriteCacheableConnectionProvider, FavoriteSettingsProvider>()
    .AddCacheLayer("pricing")
    .AddJsonSigner()
    .AddSecretEncryptor()
    .AddMinimalSecurityLayer()
    .AddIntegrationClients()
    .AddCommonLayer()
    .AddApplicationLayer(builder.Configuration)
    .AddLocalization(builder.Configuration);

builder.Services.AddScoped<IStartupTask, MarkupInitializationStartupTask>();
builder.Services.AddHostedService<StartupTaskHostedService>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

var endpointAssembly = typeof(Program).Assembly;
builder.Services.AddCarter(
    new DependencyContextAssemblyCatalog(endpointAssembly),
    c => c.WithEmptyValidators());


var app = builder.Build();

app.UseCommonApiPipeline();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

await app.RunAsync();