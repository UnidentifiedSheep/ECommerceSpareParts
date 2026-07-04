using System.Reflection;
using Abstractions;
using Analytics.Api.Consumers;
using Analytics.Api.Hubs;
using Analytics.Application;
using Analytics.Application.Consumers;
using Analytics.Cache;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Api.Common;
using Api.Common.Consumers;
using Api.Common.Extensions;
using Api.Common.Hubs;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Cache;
using Carter;
using Contracts.Job;
using Contracts.Settings;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using RabbitMq.Extensions;
using Security;
using ZiggyCreatures.Caching.Fusion.Backplane;

var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();

var builder = WebApplication.CreateBuilder(args);

var env = builder.AddServiceConfiguration("analytics");

builder.Host.AddLokiLogger(
    builder.Configuration,
    "analytics.api",
    env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions();

builder.Services.AddCommonApiInfrastructure();

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("analytics")
    .AddApplicationCache()
    .AddApplicationLayer(builder.Configuration)
    .AddIntegrationClients()
    .AddEComAuth(builder.Configuration)
    .AddMinimalSecurityLayer();

builder.Services.AddLocalization(builder.Configuration);

var uniqQueueName = $"queue-of-analytics-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(CurrencyRatesChangedConsumer)));
    x.AddConsumer<BackplaneConsumer>();
    x.AddConsumer<JobStatusUpdatedConsumer>();
    x.AddConsumer<SettingUpdatedConsumer>();
    x.AddConsumer<MetricCalculationStatusUpdatedConsumer>();

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
                ep.Bind<BackplaneMessage>();

                ep.ConfigureConsumer<MetricCalculationStatusUpdatedConsumer>(context);
                ep.Bind<MetricCalculationStatusUpdatedConsumer>();

                ep.ConfigureConsumer<JobStatusUpdatedConsumer>(context);
                ep.ConfigureConsumer<SettingUpdatedConsumer>(context);

                ep.BindForService<JobStatusUpdatedEvent>(ServicesDefinitions.Analytics)
                    .BindForService<SettingUpdatedEvent>(ServicesDefinitions.Analytics);
            });

        cfg.ReceiveEndpoint(
            "analytics-queue",
            ep =>
            {
                ep.Durable = true;

                ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
                ep.ConfigureConsumer<SaleDeletedConsumer>(context);
                ep.ConfigureConsumer<SaleUpdatedConsumer>(context);

                ep.ConfigureConsumer<PurchaseDeletedConsumer>(context);
                ep.ConfigureConsumer<PurchaseUpdatedConsumer>(context);
            });
    });
});

builder.Services.AddSignalR();

builder.Services.AddCarter(configurator: c => c.WithEmptyValidators());

var app = builder.Build();

app.UseCommonApiPipeline();

app.MapHub<MetricCalculationHub>("/hubs/calculation-jobs");

await app.LoadLocalesFromJson(localesPath);
app.MapHub<JobHub>("/hubs/jobs");
app.MapHub<JobHub>("/hubs/metrics");

await app.RunAsync();