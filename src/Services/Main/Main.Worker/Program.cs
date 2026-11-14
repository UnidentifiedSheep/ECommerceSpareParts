using Application.Common.Host;
using System.Reflection;
using Abstractions;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Cache;
using Contracts.Job;
using Contracts.Producer;
using Contracts.Settings;
using Integrations.ExchangeRate.Di;
using Main.Api;
using Main.Application;
using Main.Application.Consumers;
using Main.Cache;
using Main.Modules.Currency;
using Main.Modules.Product;
using Main.Modules.Storage;
using Contracts.Analytics;
using Pricing.Application.Consumers;
using Pricing.Module;
using Main.Persistence;
using MassTransit;
using Notification.Extensions;
using Persistence.Context;
using RabbitMQ.Client;
using RabbitMq.Extensions;
using S3;
using Security.Extensions;
using ZiggyCreatures.Caching.Fusion.Backplane;

var builder = Host.CreateApplicationBuilder(args);

var env = builder.AddServiceConfiguration("main");

builder
	.Services
	.AddMessageBrokerOptions()
	.AddHeaderSecretsOptions()
	.AddRedisOptions()
	.AddDatabaseOptions()
	.AddEmailOptions()
	.AddPhoneOptions()
	.AddJwtOptions()
	.AddS3Options()
	.AddS3BucketOptions()
	.AddLrtOptions()
	.AddScheduledJobEnqueuerOptions()
	.AddSystemOptions()
	.AddSecretEncryptionOptions();

builder.AddLokiLogger(
	builder.Configuration,
	"main.worker",
	env);

builder.Services.AddCommonWorkerInfrastructure(ServicesDefinitions.Main);

builder.Services.AddModule<CurrencyModule>(builder.Configuration);
builder.Services.AddModule<ProductModule>(builder.Configuration);
builder.Services.AddModule<StorageModule>(builder.Configuration);
builder.Services.AddModule<PricingModule>(builder.Configuration);

AddMassTransit(builder);

builder
	.Services
	.AddPersistenceLayer()
	.AddCacheLayer("main")
	.AddApplicationCache()
	.AddJsonSigner()
	.AddSecretEncryptor()
	.AddCommonLayer()
	.AddS3()
	.AddApplicationLayer(builder.Configuration)
	.AddWorkerSecurityLayer()
	.AddPasswordServices()
	.AddExchangeRates()
	.AddMainNotifications()
	.AddInAppNotificationHostedService()
	.AddEmailNotificationHostedService();

builder.Services.AddLrtHostedServices();

builder.Services.AddHostedService<StartupTaskHostedService>();

var host = builder.Build();

await host.RunAsync();

void AddMassTransit(IHostApplicationBuilder hostBuilder)
{
	var uniqQueueName = $"queue-of-main-worker-{Environment.MachineName}";
	hostBuilder.Services.AddMassTransit(x =>
	{
		x.AddConsumers([
			Assembly.GetAssembly(typeof(Global))!,
			.. builder.Services.GetModuleCatalog().ConsumerAssemblies]);
		x.AddConsumer<BackplaneConsumer>();
		x.AddConsumer<SettingUpdatedConsumer>();

		x.AddEntityFrameworkOutbox<UnifiedDbContext>(o =>
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

			cfg.ReceiveEndpoint(
				uniqQueueName,
				ep =>
				{
					ep.AutoDelete = true;
					ep.Durable = false;

					ep.ConfigureConsumeTopology = false;

					ep.ConfigureConsumer<SettingUpdatedConsumer>(context);

					ep.BindForService<SettingUpdatedEvent>(ServicesDefinitions.Main);

					ep.ConfigureConsumer<BackplaneConsumer>(context);
					ep.ConfigureConsumer<ProducerLookupChangedConsumer>(context);
					ep.ConfigureConsumer<MarkupRangesRefreshRequestedConsumer>(context);
					ep.Bind<BackplaneMessage>();
					ep.Bind<ProducerLookupChangedEvent>();
					ep.Bind<MarkupRangesRefreshRequestedEvent>();
				});

			cfg.ReceiveEndpoint(
				"main-worker-queue",
				ep =>
				{
					ep.Durable = true;

					ep.ConcurrentMessageLimit = 4;
					ep.PrefetchCount = 1;

					ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
					ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
					ep.ConfigureConsumer<SupplierProductsRequestedConsumer>(context);
					ep.ConfigureConsumer<PricingCurrencyRatesChangedConsumer>(context);
					ep.ConfigureConsumer<MarkupAnalyzedConsumer>(context);
					ep.ConfigureConsumer<ProductPriceOffersUpdatedConsumer>(context);
					ep.ConfigureConsumer<ApplySupplierProductsConsumer>(context);
				});
		});
	});
}
