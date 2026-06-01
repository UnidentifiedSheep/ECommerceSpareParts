using System.Reflection;
using Amazon.S3;
using Api.Common;
using Api.Common.Extensions;
using Application.Common.Backplane;
using Cache;
using Carter;
using Contracts.Auth;
using Contracts.Currency;
using Contracts.Products;
using Contracts.Settings;
using Contracts.StorageContent;
using Contracts.User;
using ExchangeRate;
using Localization.Domain.Extensions;
using Mail;
using Main.Api;
using Main.Api.EndPoints.Products;
using Main.Application;
using Main.Application.BackgroundServices;
using Main.Application.Configs;
using Main.Application.Consumers;
using Main.Application.Models;
using Main.Cache;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Microsoft.Extensions.Options;
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
    .AddJwtOptions();

builder.Services.AddCommonApiInfrastructure();

var uniqQueueName = $"queue-of-main-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(Global)));
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

            ep.ConfigureConsumeTopology = false;

            ep.ConfigureConsumer<SettingChangedConsumer>(context);
            
            ep.Bind<SettingChangedEvent>();
            
            ep.ConfigureConsumer<BackplaneConsumer>(context);
            ep.Bind<BackplaneMessage>();
        });

        cfg.ReceiveEndpoint("main-queue", ep =>
        {
            ep.Durable = true;

            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
            ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
            ep.ConfigureConsumer<ProductSizesUpdatedConsumer>(context);
            ep.ConfigureConsumer<ProductWeightUpdatedConsumer>(context);
            ep.ConfigureConsumer<ProductUpdatedConsumer>(context);
            ep.ConfigureConsumer<StorageContentUpdatedConsumer>(context);
            ep.ConfigureConsumer<RoleUpdatedConsumer>(context);
            ep.ConfigureConsumer<UserUpdatedConsumer>(context);
            ep.ConfigureConsumer<UserDiscountUpdatedConsumer>(context);
            ep.ConfigureConsumer<ProductLinkageUpdatedConsumer>(context);

            ep.Bind<CurrencyCreatedEvent>();
            ep.Bind<StorageContentUpdatedEvent>();
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

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("main")
    .AddApplicationCache()
    .AddJsonSigner(
        builder.Configuration["SignSecret"] ??
        throw new InvalidOperationException("SignSecret not found in configuration"),
        Global.JsonOptions)
    .AddFullSecurityLayer()
    .AddEComAuth(builder.Configuration)
    .AddMailLayer()
    .AddCommonLayer()
    .AddS3(sp =>
    {
        var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
        var config = new AmazonS3Config
        {
            ServiceURL = options.Url,
            ForcePathStyle = options.ForcePathStyle
        };
        return new AmazonS3Client(options.Login, options.Password, config);
    })
    .AddApplicationLayer(builder.Configuration)
    .AddLocalization(builder.Configuration)
    .AddExchangeRates();

builder.Services.AddHostedService<SearchLogBackgroundWorker>();

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
    new DependencyContextAssemblyCatalog(typeof(ProductsEndPoints).Assembly),
    c => c.WithEmptyValidators());

var app = builder.Build();

SortByConfig.Configure();

app.UseCommonApiPipeline();

var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
await app.LoadLocalesFromJson(localesPath);

app.UseOpenTelemetryPrometheusScrapingEndpoint();

await app.RunAsync();