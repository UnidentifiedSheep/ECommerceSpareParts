using Main.Modules.Currency.Abstractions;
using Main.Modules.Product.Application.Events;
using Pricing.Application.Events;
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
using Cache.OneTimeTokens;
using Main.Modules.Currency;
using Main.Modules.Product;
using Main.Modules.Identity;
using Main.Modules.Purchase;
using Main.Modules.Analytics;
using Main.Modules.Balance;
using Main.Modules.Cart;
using Main.Modules.Currency.Abstractions.Events;
using Main.Modules.Currency.Application.Consumers;
using Main.Modules.Documents;
using Main.Modules.Jobs;
using Main.Modules.Platform;
using Main.Modules.Suppliers;
using Main.Modules.Jobs.Application;
using Main.Modules.Notifications;
using Main.Modules.Notifications.Application.Delivery.Extensions;
using Main.Modules.Order;
using Main.Modules.Organization;
using Main.Modules.Platform.Application.Consumer;
using Main.Modules.Product.Application.Consumers;
using Main.Modules.Sale;
using Main.Modules.Storage;
using Pricing.Application.Consumers;
using Pricing.Module;
using Persistence.Common;
using Persistence.Relations;
using MassTransit;
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
	.AddJobsOptions()
	.AddSystemOptions()
	.AddSecretEncryptionOptions();

builder.AddLokiLogger(
	builder.Configuration,
	"main.worker",
	env);

builder.Services.AddCommonWorkerInfrastructure(ServicesDefinitions.Main);

builder.Services.AddModule<CurrencyModule>(builder.Configuration);
builder.Services.AddModule<ProductModule>(builder.Configuration);
builder.Services.AddModule<IdentityModule>(builder.Configuration);
builder.Services.AddModule<PurchaseModule>(builder.Configuration);
builder.Services.AddModule<SaleModule>(builder.Configuration);
builder.Services.AddModule<AnalyticsModule>(builder.Configuration);
builder.Services.AddModule<BalanceModule>(builder.Configuration);
builder.Services.AddModule<CartModule>(builder.Configuration);
builder.Services.AddModule<DocumentsModule>(builder.Configuration);
builder.Services.AddModule<JobsModule>(builder.Configuration);
builder.Services.AddModule<PlatformModule>(builder.Configuration);
builder.Services.AddModule<SuppliersModule>(builder.Configuration);
builder.Services.AddModule<NotificationsModule>(builder.Configuration);
builder.Services.AddModule<OrderModule>(builder.Configuration);
builder.Services.AddModule<OrganizationModule>(builder.Configuration);
builder.Services.AddModule<StorageModule>(builder.Configuration);
builder.Services.AddModule<PricingModule>(builder.Configuration);

AddMassTransit(builder);

builder
	.Services
	.AddPersistenceCore()
	.AddRelationsPersistence()
	.AddCacheLayer("main")
	.AddOneTimeTokens()
	.AddJsonSigner()
	.AddSecretEncryptor()
	.AddCommonLayer()
	.AddS3()
	.AddWorkerSecurityLayer()
	.AddPasswordServices()
	.AddExchangeRates()
	.UseEmailNotifications()
	.AddInAppNotificationHostedService()
	.AddEmailNotificationHostedService();

builder.Services.AddJobsHostedServices();

builder.Services.AddHostedService<StartupTaskHostedService>();

var host = builder.Build();

await host.RunAsync();

void AddMassTransit(IHostApplicationBuilder hostBuilder)
{
	var uniqQueueName = $"queue-of-main-worker-{Environment.MachineName}";
	hostBuilder.Services.AddMassTransit(x =>
	{
		x.AddConsumers([
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
					ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
					ep.ConfigureConsumer<PricingCurrencyRatesChangedConsumer>(context);
					ep.ConfigureConsumer<MarkupRangesRefreshRequestedConsumer>(context);
					ep.Bind<BackplaneMessage>();
					ep.Bind<ProducerLookupChangedEvent>();
					ep.Bind<CurrencyRateChangedEvent>();
					ep.Bind<MarkupRangesRefreshRequestedEvent>();
				});

			cfg.ReceiveEndpoint(
				"main-worker-queue",
				ep =>
				{
					ep.Durable = true;

					ep.ConcurrentMessageLimit = 4;
					ep.PrefetchCount = 1;

					ep.ConfigureConsumer<SupplierProductsRequestedConsumer>(context);
					ep.ConfigureConsumer<MarkupAnalyzedConsumer>(context);
					ep.ConfigureConsumer<ProductPriceOffersUpdatedConsumer>(context);
					ep.ConfigureConsumer<ApplySupplierProductsConsumer>(context);
				});
		});
	});
}
