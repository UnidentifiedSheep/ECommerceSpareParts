using System.Reflection;
using Abstractions;
using Api.Common;
using Api.Common.Consumers;
using Api.Common.EndPoints;
using Api.Common.Extensions;
using Api.Common.Hubs;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Cache;
using Carter;
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
using Main.Api.EndPoints.Products;
using Main.Application;
using Main.Application.Configs;
using Main.Application.Consumers;
using Main.Cache;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using OpenTelemetry.Metrics;
using RabbitMq.Extensions;
using S3;
using Security;
using ZiggyCreatures.Caching.Fusion.Backplane;
using Global = Main.Application.Global;

var builder = WebApplication.CreateBuilder(args);

var env = builder.AddServiceConfiguration("main");

builder.Host.AddLokiLogger(
    builder.Configuration,
    "main.api",
    env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddS3Options()
    .AddDatabaseOptions()
    .AddEmailOptions()
    .AddPhoneOptions()
    .AddJwtOptions()
    .AddSystemOptions()
    .AddSecretEncryptionOptions();

builder.Services.AddCommonApiInfrastructure();
builder.Services.AddSignalR();

var uniqQueueName = $"queue-of-main-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(Global)));
    x.AddConsumer<BackplaneConsumer>();
    x.AddConsumer<JobStatusUpdatedConsumer>();
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

                ep.ConfigureConsumeTopology = false;

                ep.ConfigureConsumer<SettingUpdatedConsumer>(context);
                ep.ConfigureConsumer<BackplaneConsumer>(context);
                ep.ConfigureConsumer<JobStatusUpdatedConsumer>(context);

                ep.Bind<BackplaneMessage>();
                ep.BindForService<JobStatusUpdatedEvent>(ServicesDefinitions.Main)
                    .BindForService<SettingUpdatedEvent>(ServicesDefinitions.Main);
            });

        cfg.ReceiveEndpoint(
            "main-queue",
            ep =>
            {
                ep.Durable = true;

                ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
                ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
                ep.ConfigureConsumer<ProductUpdatedConsumer>(context);
                ep.ConfigureConsumer<RoleUpdatedConsumer>(context);
                ep.ConfigureConsumer<UserUpdatedConsumer>(context);
                ep.ConfigureConsumer<UserDiscountUpdatedConsumer>(context);

                ep.Bind<CurrencyCreatedEvent>();
                ep.Bind<ProductUpdatedEvent>();
                ep.Bind<RoleUpdatedEvent>();
                ep.Bind<UserUpdatedEvent>();
                ep.Bind<UserDiscountUpdatedEvent>();
                ep.Bind<CurrencyRateChangedEvent>();
            });
    });
});

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("main")
    .AddApplicationCache()
    .AddJsonSigner()
    .AddSecretEncryptor()
    .AddFullSecurityLayer()
    .AddEComAuth(builder.Configuration)
    .AddMailLayer()
    .AddCommonLayer()
    .AddS3()
    .AddApplicationLayer(builder.Configuration)
    .AddLocalization(builder.Configuration)
    .AddExchangeRates();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

builder.Services.AddCarter(
    new DependencyContextAssemblyCatalog(
        typeof(ProductsEndPoints).Assembly,
        typeof(JobEndPoints).Assembly),
    c => c.WithEmptyValidators());

var app = builder.Build();

SortByConfig.Configure();

app.UseCommonApiPipeline();

var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
await app.LoadLocalesFromJson(localesPath);

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapHub<JobHub>("/hubs/jobs");

await app.RunAsync();