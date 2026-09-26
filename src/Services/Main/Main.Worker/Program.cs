using System.Reflection;
using Abstractions;
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
using Contracts.Supplier;
using Contracts.User;
using ExchangeRate;
using Main.Api;
using Main.Application;
using Main.Application.Consumers;
using Main.Cache;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Notification.Extensions;
using RabbitMQ.Client;
using RabbitMq.Extensions;
using S3;
using Security;
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
	.AddFullSecurityLayer()
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

			cfg.ReceiveEndpoint(
				uniqQueueName,
				ep =>
				{
					ep.AutoDelete = true;
					ep.Durable = false;

					ep.ConfigureConsumeTopology = false;

					ep.ConfigureConsumer<SettingUpdatedConsumer>(context);

					ep.Bind<SettingUpdatedEvent>();

					ep.ConfigureConsumer<BackplaneConsumer>(context);
					ep.Bind<BackplaneMessage>();
				});

			cfg.ReceiveEndpoint(
				"main-worker-queue",
				ep =>
				{
					ep.Durable = true;

					ep.ConcurrentMessageLimit = 4;
					ep.PrefetchCount = 1;

					ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
					ep.ConfigureConsumer<RoleUpdatedConsumer>(context);
					ep.ConfigureConsumer<UserDiscountUpdatedConsumer>(context);
					ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
					ep.ConfigureConsumer<SupplierProductsRequestedConsumer>(context);

					ep.Bind<SupplierProductsRequestedEvent>();
					ep.Bind<CurrencyCreatedEvent>();
					ep.Bind<ProductUpdatedEvent>();
					ep.Bind<RoleUpdatedEvent>();
					ep.Bind<UserDiscountUpdatedEvent>();
					ep.Bind<CurrencyRateChangedEvent>();
				});
		});
	});
}
