using Main.Modules.Currency.Abstractions;
using Main.Modules.Product.Application.Events;
using Pricing.Application.Events;
using Notification.Extensions;
using Application.Common.Host;
using System.Reflection;
using Abstractions;
using Api.Common;
using Api.Common.Consumers;
using Api.Common.EndPoints;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Api.Common.Hubs;
using Application.Common.Backplane;
using Application.Common.Consumer;
using Cache;
using Carter;
using Contracts.Job;
using Contracts.Producer;
using Contracts.Settings;
using GraphQL.Common.Extensions;
using Integrations.ExchangeRate.Di;
using Main.Api;
using Main.Api.EndPoints;
using Main.Api.GraphQl;
using Main.Application.Consumers;
using Cache.OneTimeTokens;
using Main.Modules.Currency;
using Main.Modules.Product;
using Main.Modules.Identity;
using Main.Modules.Purchase;
using Main.Modules.Analytics;
using Main.Modules.Balance;
using Main.Modules.Cart;
using Main.Modules.Documents;
using Main.Modules.Jobs;
using Main.Modules.Platform;
using Main.Modules.Suppliers;
using Main.Modules.Uploads;
using Main.Modules.Notifications;
using Main.Modules.Order;
using Main.Modules.Organization;
using Main.Modules.Sale;
using Main.Modules.Storage;
using Pricing.Application.Consumers;
using Pricing.Module;
using Persistence.Common;
using Persistence.Relations;
using MassTransit;
using Persistence.Context;
using RabbitMq.Extensions;
using S3;
using Security.Extensions;
using ZiggyCreatures.Caching.Fusion.Backplane;

const string serviceName = "Main";
var builder = WebApplication.CreateBuilder(args);

var env = builder.AddServiceConfiguration("main");

builder.Host.AddLokiLogger(
	builder.Configuration,
	"main.api",
	env);

builder
	.Services
	.AddMessageBrokerOptions()
	.AddHeaderSecretsOptions()
	.AddRedisOptions()
	.AddS3Options()
	.AddDatabaseOptions()
	.AddEmailOptions()
	.AddPhoneOptions()
	.AddJwtOptions()
	.AddS3BucketOptions()
	.AddSystemOptions()
	.AddSecretEncryptionOptions();

builder.Services.AddCommonApiInfrastructure(ServicesDefinitions.Main);
builder.Services.AddPostgresHealthCheck().AddRedisHealthCheck();
builder.Services.AddSignalR();

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
builder.Services.AddModule<UploadsModule>(builder.Configuration);
builder.Services.AddModule<NotificationsModule>(builder.Configuration);
builder.Services.AddModule<OrderModule>(builder.Configuration);
builder.Services.AddModule<OrganizationModule>(builder.Configuration);
builder.Services.AddModule<StorageModule>(builder.Configuration);
builder.Services.AddModule<PricingModule>(builder.Configuration);

var uniqQueueName = $"queue-of-main-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
	x.AddConsumers([
		.. builder.Services.GetModuleCatalog().ConsumerAssemblies]);
	x.AddConsumer<BackplaneConsumer>();
	x.AddConsumer<JobStatusUpdatedConsumer>();
	x.AddConsumer<SettingUpdatedConsumer>();

	x.AddEntityFrameworkOutbox<UnifiedDbContext>(o =>
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
				ep.ConfigureConsumer<ProducerLookupChangedConsumer>(context);
				ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
				ep.ConfigureConsumer<PricingCurrencyRatesChangedConsumer>(context);
				ep.ConfigureConsumer<MarkupRangesRefreshRequestedConsumer>(context);
				ep.ConfigureConsumer<JobStatusUpdatedConsumer>(context);

				ep.Bind<BackplaneMessage>();
				ep.Bind<ProducerLookupChangedEvent>();
				ep.Bind<CurrencyRateChangedEvent>();
				ep.Bind<MarkupRangesRefreshRequestedEvent>();
				ep
					.BindForService<JobStatusUpdatedEvent>(ServicesDefinitions.Main)
					.BindForService<SettingUpdatedEvent>(ServicesDefinitions.Main);
			});
	});
});

builder
	.Services
	.AddPersistenceCore()
	.AddRelationsPersistence()
	.AddCacheLayer("main")
	.AddOneTimeTokens()
	.AddJsonSigner()
	.AddSecretEncryptor()
	.AddFullSecurityLayer()
	.AddEComAuth(builder.Configuration)
	.AddCommonLayer()
	.AddS3()
	.AddExchangeRates()
	.UseEmailNotifications();

builder.Services.AddCarter(
	new DependencyContextAssemblyCatalog(
		typeof(AuthEndPoints).Assembly,
		typeof(JobEndPoints).Assembly),
	c => c.WithEmptyValidators());

builder.Services.AddGraphQlServices(serviceName);

builder.Services.AddHostedService<StartupTaskHostedService>();

var app = builder.Build();

app.UseCommonApiPipeline();

app.MapHub<JobHub>("/hubs/jobs");
app.MapCommonGraphQl();

await app.RunWithGraphQLCommandsAsync(args);
