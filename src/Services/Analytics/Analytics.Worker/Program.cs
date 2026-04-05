using System.Reflection;
using Analytics.Application;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Analytics.Worker.Consumers;
using Api.Common.Extensions;
using Localization.Abstractions.Models;
using Localization.Domain.Extensions;
using MassTransit;
using RabbitMq.Extensions;
using RabbitMq.Models;
using Redis;

Locale[] locales = ["ru-RU", "en-EN"];
Locale defaultLocale = "ru-RU";

var builder = Host.CreateApplicationBuilder(args);

AddLoki(builder);

builder.Services.AddLocalization(defaultLocale, locales);

builder.Services
    .AddPersistenceLayer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!)
    .AddCacheLayer(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!, "analytics")
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
    var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown";

    hostBuilder.AddLokiLogger(hostBuilder.Configuration, "analytics.worker", env, lokiUrl);
}

void AddMassTransit(IHostApplicationBuilder hostBuilder)
{
    hostBuilder.Services.AddOptions<MessageBrokerOptions>()
        .BindConfiguration(MessageBrokerOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var brokerOptions = hostBuilder.Configuration
                            .GetSection(MessageBrokerOptions.SectionName)
                            .Get<MessageBrokerOptions>()
                        ?? throw new NullReferenceException($"Missing {MessageBrokerOptions.SectionName} configuration options");
    
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
            cfg.ConfigureRabbitMq(brokerOptions);
            
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