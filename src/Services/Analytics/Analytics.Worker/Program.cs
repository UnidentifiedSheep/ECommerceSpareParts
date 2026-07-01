using System.Reflection;
using Abstractions;
using Analytics.Application;
using Analytics.Cache;
using Analytics.Persistence;
using Analytics.Persistence.Context;
using Analytics.Worker;
using Analytics.Worker.HostedServices;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Cache;
using Contracts.Job;
using Contracts.Settings;
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

builder.Services.AddCommonWorkerInfrastructure();

builder.AddLokiLogger(
    builder.Configuration,
    "analytics.worker",
    env);

builder.Services.AddLocalization(builder.Configuration);

builder.Services
    .AddPersistenceLayer()
    .AddApplicationCache()
    .AddCacheLayer("analytics")
    .AddIntegrationClients()
    .AddApplicationLayer(builder.Configuration)
    .AddWorkerSecurityLayer()
    .AddLrtOptions()
    .AddScheduledJobEnqueuerOptions()
    .AddSystemOptions();

AddHostedServiceOptions(builder.Services);
builder.Services
    .AddHostedService<RecalculationCheckHostedService>()
    .AddHostedService<LrtExecutorHostedService>()
    .AddHostedService<ScheduledJobEnqueuerHostedService>();

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

                    ep.ConfigureConsumer<BackplaneConsumer>(context);
                    ep.ConfigureConsumer<SettingUpdatedConsumer>(context);

                    ep.Bind<BackplaneMessage>();

                    ep.BindForService<JobStatusUpdatedEvent>(ServicesDefinitions.Analytics)
                        .BindForService<SettingUpdatedEvent>(ServicesDefinitions.Analytics);
                });

            cfg.ReceiveEndpoint("analytics-work-queue", ep => { ep.Durable = true; });
        });
    });
}