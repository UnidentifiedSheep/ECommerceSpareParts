using System.Reflection;
using Analytics.Application;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Analytics.Worker.Consumers;
using Api.Common;
using Api.Common.Extensions;
using Cache;
using Common;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using RabbitMq.Extensions;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddAppSettingsFromJsons(env)
    .AddAppSettingsFromJsons(env, "/app/configs")
    .AddConfigsFromJsons("analytics", env, "/app/configs");

builder.Services
    .AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions();

AddLoki(builder);

builder.Services.AddLocalization(builder.Configuration);

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("analytics")
    .AddIntegrationClients()
    .AddApplicationLayer();

AddMassTransit(builder);

var host = builder.Build();

await LoadLocalization(host);

await host.RunAsync();

return;

async Task LoadLocalization(IHost hostApp)
{
    var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
    await hostApp.LoadLocalesFromJson(localesPath);
}

void AddLoki(IHostApplicationBuilder hostBuilder)
{
    hostBuilder.AddLokiLogger(
        hostBuilder.Configuration,
        "analytics.worker",
        env);
}

void AddMassTransit(IHostApplicationBuilder hostBuilder)
{
    hostBuilder.Services.AddMassTransit(x =>
    {
        x.AddConsumers(Assembly.GetAssembly(typeof(MetricCalculationRequestedConsumer)));

        x.AddEntityFrameworkOutbox<DContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.ConfigureRabbitMq(context);

            cfg.ReceiveEndpoint("analytics-work-queue", ep =>
            {
                ep.Durable = true;

                ep.ConcurrentMessageLimit = 4;
                ep.PrefetchCount = 4;

                ep.ConfigureConsumer<MetricCalculationRequestedConsumer>(context);
            });
        });
    });
}