using System.Reflection;
using Abstractions;
using Analytics.Api.Hubs;
using Analytics.Application;
using Analytics.Application.Consumers;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Api.Common;
using Api.Common.Consumers;
using Api.Common.Extensions;
using Api.Common.Hubs;
using Application.Common.Backplane;
using Cache;
using Carter;
using Contracts.Job;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using RabbitMQ.Client;
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
    .AddApplicationLayer(builder.Configuration)
    .AddIntegrationClients()
    .AddEComAuth(builder.Configuration)
    .AddMinimalSecurityLayer();

builder.Services.AddLocalization(builder.Configuration);

var uniqQueueName = $"queue-of-analytics-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(CurrencyCreatedConsumer)));
    x.AddConsumer<BackplaneConsumer>();
    x.AddConsumer<JobStatusUpdatedConsumer>();

    x.AddEntityFrameworkOutbox<DContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);
        cfg.Publish<JobStatusUpdatedEvent>(p =>
        {
            p.ExchangeType = ExchangeType.Direct;
        });

        cfg.ReceiveEndpoint(uniqQueueName, ep =>
        {
            ep.AutoDelete = true;
            ep.Durable = false;
            ep.ConfigureConsumeTopology = false;
            
            ep.ConfigureConsumer<BackplaneConsumer>(context);
            ep.Bind<BackplaneMessage>();

            ep.ConfigureConsumer<JobStatusUpdatedConsumer>(context);
            
            ep.Bind<JobStatusUpdatedEvent>(bind =>
            {
                bind.ExchangeType = ExchangeType.Direct;
                bind.RoutingKey = ServicesDefinitions.Analytics.ServiceName;
            });
        });

        cfg.ReceiveEndpoint("analytics-queue", ep =>
        {
            ep.Durable = true;

            ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
            ep.ConfigureConsumer<SaleDeletedConsumer>(context);
            ep.ConfigureConsumer<SaleUpdatedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);

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

await app.RunAsync();
