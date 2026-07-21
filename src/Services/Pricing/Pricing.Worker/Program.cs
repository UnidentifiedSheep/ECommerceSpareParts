using System.Reflection;
using Abstractions;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Api.Common.HostedServices.Startup;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Application.Common.Interfaces;
using Application.Common.Services.Supplier;
using Cache;
using Contracts.Analytics;
using Contracts.Job;
using Contracts.Settings;
using Integrations.Supplier.DI;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using Pricing.Api.Startup;
using Pricing.Application;
using Pricing.Application.Consumers;
using Pricing.Cache;
using Pricing.Persistence;
using Pricing.Persistence.Contexts;
using RabbitMq.Extensions;
using Security;
using ZiggyCreatures.Caching.Fusion.Backplane;

var builder = Host.CreateApplicationBuilder(args);

var env = builder.AddServiceConfiguration("pricing");

builder.Services
    .AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions()
    .AddLrtOptions()
    .AddScheduledJobEnqueuerOptions()
    .AddSystemOptions()
    .AddSecretEncryptionOptions();

builder.Services.AddCommonWorkerInfrastructure();

builder.AddLokiLogger(
    builder.Configuration,
    "pricing.worker",
    env);

builder.Services.AddLocalization(builder.Configuration);

builder.Services
    .AddPersistenceLayer()
    .AddApplicationCache()
    .AddCacheLayer("pricing")
    .AddIntegrationClients()
    .AddApplicationLayer(builder.Configuration)
    .AddCommonLayer()
    .AddWorkerSecurityLayer()
    .AddFavoriteIntegration<FavoriteCacheableConnectionProvider, FavoriteSettingsProvider>()
    .AddJsonSigner()
    .AddSecretEncryptor();

builder.Services.AddScoped<IStartupTask, MarkupInitializationStartupTask>()
    .AddScoped<IStartupTask, LoadLocalesStartupTask>()
    .AddHostedService<StartupTaskHostedService>()
    .AddLrtHostedServices();

var uniqQueueName = $"queue-of-pricing-worker-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(Global)));
    x.AddConsumer<BackplaneConsumer>();
    x.AddConsumer<SettingUpdatedConsumer>();
    x.AddConsumer<ProductPriceOffersUpdatedConsumer, ProductPriceOffersUpdatedDefinition>();
    x.AddConsumer<StorageContentUpdatedConsumer, StorageContentUpdatedDefinition>();

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
            "pricing-worker-queue",
            ep =>
            {
                ep.Durable = true;

                ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
                ep.ConfigureConsumer<MarkupAnalyzedConsumer>(context);
                ep.ConfigureConsumer<ProductPriceOffersUpdatedConsumer>(context);
                ep.ConfigureConsumer<StorageContentUpdatedConsumer>(context);
                ep.ConfigureConsumer<SupplierProductsRequestedConsumer>(context);
            });
    });
});

var host = builder.Build();
host.Run();