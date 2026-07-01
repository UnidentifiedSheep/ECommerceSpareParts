using System.Reflection;
using Amazon.S3;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Cache;
using Contracts.Auth;
using Contracts.Currency;
using Contracts.Job;
using Contracts.Products;
using Contracts.Settings;
using Contracts.User;
using ExchangeRate;
using Localization.Domain.Extensions;
using Mail;
using Main.Api;
using Main.Application;
using Main.Application.Consumers;
using Main.Cache;
using Main.Persistence;
using Main.Persistence.Context;
using Main.Worker;
using Main.Worker.HostedServices;
using MassTransit;
using Microsoft.Extensions.Options;
using Persistence;
using RabbitMq.Extensions;
using RabbitMQ.Client;
using S3;
using Security;
using ZiggyCreatures.Caching.Fusion.Backplane;

var builder = Host.CreateApplicationBuilder(args);

var env = builder.AddServiceConfiguration("main");

builder.Services
    .AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions()
    .AddEmailOptions()
    .AddPhoneOptions()
    .AddJwtOptions()
    .AddS3Options()
    .AddLrtOptions()
    .AddScheduledJobEnqueuerOptions()
    .AddSystemOptions()
    .AddSecretEncryptionOptions();

builder.AddLokiLogger(
    builder.Configuration,
    "main.worker",
    env);

builder.Services.AddCommonWorkerInfrastructure();

AddMassTransit(builder);

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("main")
    .AddApplicationCache()
    .AddJsonSigner()
    .AddSecretEncryptor()
    .AddMailLayer()
    .AddCommonLayer()
    .AddS3()
    .AddApplicationLayer(builder.Configuration)
    .AddLocalization(builder.Configuration)
    .AddWorkerSecurityLayer()
    .AddFullSecurityLayer()
    .AddExchangeRates();

AddHostedServiceOptions(builder.Services);
builder.Services
    .AddHostedService<EmailWorkHostedService>()
    .AddHostedService<LrtExecutorHostedService>()
    .AddHostedService<ScheduledJobEnqueuerHostedService>();

var host = builder.Build();

await host.LoadLocalesFromJson(Assembly.GetExecutingAssembly().GetDefaultLocalizationPath());

await host.RunAsync();

void AddHostedServiceOptions(IServiceCollection collection)
{
    collection.AddOptions<HostedServiceOptions>()
        .BindConfiguration(HostedServiceOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

void AddMassTransit(IHostApplicationBuilder hostBuilder)
{
    var uniqQueueName = $"queue-of-main-worker-{Environment.MachineName}";
    hostBuilder.Services.AddMassTransit(x =>
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
            cfg.Publish<JobStatusUpdatedEvent>(p =>
            {
                p.ExchangeType = ExchangeType.Direct;
            });

            cfg.ReceiveEndpoint(uniqQueueName, ep =>
            {
                ep.AutoDelete = true;
                ep.Durable = false;

                ep.ConfigureConsumeTopology = false;

                ep.ConfigureConsumer<SettingUpdatedConsumer>(context);
                
                ep.Bind<SettingUpdatedEvent>();
                
                ep.ConfigureConsumer<BackplaneConsumer>(context);
                ep.Bind<BackplaneMessage>();
            });

            cfg.ReceiveEndpoint("main-worker-queue", ep =>
            {
                ep.Durable = true;
                
                ep.ConcurrentMessageLimit = 4;
                ep.PrefetchCount = 1;
                
                ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
                ep.ConfigureConsumer<ProductSizesUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductWeightUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductUpdatedConsumer>(context);
                ep.ConfigureConsumer<RoleUpdatedConsumer>(context);
                ep.ConfigureConsumer<UserUpdatedConsumer>(context);
                ep.ConfigureConsumer<UserDiscountUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductLinkageUpdatedConsumer>(context);
                ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);

                
                ep.Bind<CurrencyCreatedEvent>();
                ep.Bind<ProductSizesUpdatedEvent>();
                ep.Bind<ProductWeightUpdatedEvent>();
                ep.Bind<ProductUpdatedEvent>();
                ep.Bind<RoleUpdatedEvent>();
                ep.Bind<UserUpdatedEvent>();
                ep.Bind<UserDiscountUpdatedEvent>();
                ep.Bind<ProductLinkageUpdatedEvent>();
                ep.Bind<CurrencyRateChangedEvent>();
            });
        });
    });
}
