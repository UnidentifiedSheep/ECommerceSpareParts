using System.Reflection;
using Analytics.Application;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Analytics.Worker;
using Analytics.Worker.Consumers;
using Analytics.Worker.HostedServices;
using Api.Common;
using Api.Common.Extensions;
using Application.Common.Backplane;
using Cache;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using RabbitMq.Extensions;
using Security;
using ZiggyCreatures.Caching.Fusion.Backplane;

var builder = Host.CreateApplicationBuilder(args);

var env = builder.AddServiceConfiguration("analytics");

builder.Services
    .AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions();

builder.AddLokiLogger(
    builder.Configuration,
    "analytics.worker",
    env);

builder.Services.AddLocalization(builder.Configuration);

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("analytics")
    .AddIntegrationClients()
    .AddApplicationLayer(builder.Configuration)
    .AddWorkerSecurityLayer();

AddHostedServiceOptions(builder.Services);
builder.Services.AddHostedService<RecalculationCheckHostedService>();

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

void AddHostedServiceOptions(IServiceCollection collection)
{
    collection.AddOptions<HostedServiceOptions>()
        .BindConfiguration(HostedServiceOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

void AddMassTransit(IHostApplicationBuilder hostBuilder)
{
    var uniqQueueName = $"queue-of-analytics-worker-{Environment.MachineName}";
    hostBuilder.Services.AddMassTransit(x =>
    {
        x.AddConsumers(Assembly.GetAssembly(typeof(MetricCalculationRequestedConsumer)));
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

            cfg.ReceiveEndpoint("analytics-work-queue", ep =>
            {
                ep.Durable = true;

                ep.ConcurrentMessageLimit = 4;
                ep.PrefetchCount = 1;
                
                ep.ConfigureConsumer<MetricCalculationRequestedConsumer>(context);
            });
        });
    });
}
