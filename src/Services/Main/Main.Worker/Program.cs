using System.Reflection;
using Amazon.S3;
using Api.Common;
using Api.Common.Extensions;
using Application.Common.Backplane;
using Cache;
using Contracts.Auth;
using Contracts.Currency;
using Contracts.Products;
using Contracts.Settings;
using Contracts.StorageContent;
using Contracts.User;
using ExchangeRate;
using Hangfire;
using Hangfire.PostgreSql;
using Localization.Domain.Extensions;
using Mail;
using Main.Api;
using Main.Application;
using Main.Application.Consumers;
using Main.Application.HangFireTasks;
using Main.Cache;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Microsoft.Extensions.Options;
using Persistence;
using RabbitMq.Extensions;
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
    .AddJwtOptions();

builder.AddLokiLogger(
    builder.Configuration,
    "main.worker",
    env);

AddMassTransit(builder);

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("main")
    .AddApplicationCache()
    .AddJsonSigner(
        builder.Configuration["SignSecret"] ??
        throw new InvalidOperationException("SignSecret not found in configuration"),
        Global.JsonOptions)
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
    .AddWorkerSecurityLayer()
    .AddFullSecurityLayer()
    .AddExchangeRates();

builder.Services.AddHangfire((sp, x) =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    x.UsePostgreSqlStorage(z =>
        z.UseNpgsqlConnection(options.ConnectionString));
});

builder.Services.AddHangfireServer();

var host = builder.Build();

await host.LoadLocalesFromJson(Assembly.GetExecutingAssembly().GetDefaultLocalizationPath());

using (var scope = host.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<UpdateCurrencyRate>("UpdateCurrencyTask",
        x => x.Run(), Cron.Daily);

    recurringJobManager.AddOrUpdate<NotifySuggestionsRebuildNeeded>("RebuildSuggestionsTask",
        x => x.Run(), Cron.Daily);
}

await host.RunAsync();

void AddMassTransit(IHostApplicationBuilder hostBuilder)
{
    var uniqQueueName = $"queue-of-main-worker-{Environment.MachineName}";
    hostBuilder.Services.AddMassTransit(x =>
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

                ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
                ep.ConfigureConsumer<ProductSizesUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductWeightUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductUpdatedConsumer>(context);
                ep.ConfigureConsumer<StorageContentUpdatedConsumer>(context);
                ep.ConfigureConsumer<RoleUpdatedConsumer>(context);
                ep.ConfigureConsumer<UserUpdatedConsumer>(context);
                ep.ConfigureConsumer<UserDiscountUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductLinkageUpdatedConsumer>(context);
                ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);

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
}
