using System.Reflection;
using Analytics.Application;
using Analytics.Application.Consumers;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.Middleware;
using Api.Common.Models;
using Api.Common.Models.Options;
using Cache;
using Carter;
using Common;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using Localization.Domain.Middlewares;
using MassTransit;
using Persistence.Extensions;
using RabbitMq;
using RabbitMq.Extensions;
using Security;

var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();

var builder = WebApplication.CreateBuilder(args);

var env = builder.AddServiceConfiguration("analytics");

builder.Host.AddLokiLogger(
    configuration: builder.Configuration, 
    serviceName: "analytics.api", 
    environment:env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions();

builder.Services.AddCommonApiInfrastructure();

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("analytics")
    .AddApplicationLayer()
    .AddIntegrationClients()
    .AddEComAuth(builder.Configuration)
    .AddMinimalSecurityLayer();

builder.Services.AddLocalization(builder.Configuration);

var uniqQueueName = $"queue-of-analytics-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(CurrencyCreatedConsumer)));

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
