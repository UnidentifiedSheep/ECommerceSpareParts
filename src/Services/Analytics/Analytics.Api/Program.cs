using System.Reflection;
using Analytics.Application;
using Analytics.Application.Consumers;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Api.Common;
using Api.Common.Extensions;
using Application.Common.Backplane;
using Cache;
using Carter;
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

    x.AddEntityFrameworkOutbox<DContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);

        cfg.ReceiveEndpoint(uniqQueueName, ep =>
        {
            ep.AutoDelete = true;
            ep.Durable = false;
            
            ep.ConfigureConsumer<BackplaneConsumer>(context);
            ep.Bind<BackplaneMessage>();
        });

        cfg.ReceiveEndpoint("analytics-queue", ep =>
        {
            ep.Durable = true;

            ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
            ep.ConfigureConsumer<SaleCreatedConsumer>(context);
            ep.ConfigureConsumer<SaleDeletedConsumer>(context);
            ep.ConfigureConsumer<SaleEditedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);

            ep.ConfigureConsumer<PurchaseCreatedConsumer>(context);
            ep.ConfigureConsumer<PurchaseDeletedConsumer>(context);
            ep.ConfigureConsumer<PurchaseUpdatedConsumer>(context);
        });
    });
});

builder.Services.AddCarter(configurator: c => c.WithEmptyValidators());

var app = builder.Build();

app.UseCommonApiPipeline();

await app.LoadLocalesFromJson(localesPath);

if (app.Environment.IsDevelopment()) app.MapOpenApi();


await app.RunAsync();
