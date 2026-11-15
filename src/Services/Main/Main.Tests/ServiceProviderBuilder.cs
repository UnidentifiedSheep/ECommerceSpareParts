using System.Globalization;
using Api.Common;
using Application.Common.Host;
using Application.Common.Models.Options.S3;
using Cache;
using Locan.Hosting;
using Main.Api;
using Main.Application.Models;
using Main.Cache;
using Main.Modules.Currency;
using Main.Modules.Product;
using Main.Modules.Identity;
using Main.Modules.Purchase;
using Main.Modules.Balance;
using Main.Modules.Notifications;
using Main.Modules.Order;
using Main.Modules.Organization;
using Main.Modules.Sale;
using Main.Modules.Storage;
using Pricing.Module;
using Main.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Notification.Extensions;
using Npgsql;
using Persistence;
using Persistence.Context;
using S3.Core.Interfaces;
using Security;
using Security.Core.Interfaces;
using Security.Extensions;
using Security.Models;
using Serilog;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.Interfaces.ServiceProvider;
using Tests.Stubs;
using Tests.TestContexts;
using ZiggyCreatures.Caching.Fusion.Backplane;
using ApplicationServiceProvider = Main.Application.ServiceProvider;

namespace Tests;

public class ServiceProviderBuilder : IServiceProviderBuilder<ServiceProviderArguments>
{
	public IServiceProvider Build(ServiceProviderArguments args)
	{
		RegisterGlobalBasicContexts();
		var culture = CultureInfo.GetCultureInfo("ru-RU");
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;
		CultureInfo.CurrentCulture = culture;
		CultureInfo.CurrentUICulture = culture;

		var services = new ServiceCollection();

		services.RegisterTestContexts();
		services.AddNotifications()
			.AddNotificationServices();


		services.AddLogging();
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel
			.Debug()
			.Enrich
			.FromLogContext()
			.WriteTo
			.Console(formatProvider: CultureInfo.InvariantCulture)
			.CreateLogger();

		ApplicationServiceProvider.AddApplicationLayer(services, null).AddLocan().AddPersistenceLayer();
		services.AddModule<CurrencyModule>(new ConfigurationBuilder().Build());
		services.AddModule<ProductModule>(new ConfigurationBuilder().Build());
		services.AddModule<IdentityModule>(new ConfigurationBuilder().Build());
		services.AddModule<PurchaseModule>(new ConfigurationBuilder().Build());
		services.AddModule<SaleModule>(new ConfigurationBuilder().Build());
		services.AddModule<BalanceModule>(new ConfigurationBuilder().Build());
		services.AddModule<NotificationsModule>(new ConfigurationBuilder().Build());
		services.AddModule<OrderModule>(new ConfigurationBuilder().Build());
		services.AddModule<OrganizationModule>(new ConfigurationBuilder().Build());
		services.AddModule<StorageModule>(new ConfigurationBuilder().Build());
		services.AddModule<PricingModule>(new ConfigurationBuilder().Build());
		services.AddSingleton<TestTimeProvider>();
		services.AddSingleton<TimeProvider>(sp => sp.GetRequiredService<TestTimeProvider>());
		var passwordRules = new PasswordRules
		{
			RequireDigit = false, RequireUppercase = false
		};

		services.AddSingleton(
			Options.Create(
				new RedisOptions
				{
					Url = args.CacheConnectionString, Password = null
				}));

		services.AddSingleton(
			Options.Create(
				new S3BucketsOptions
				{
					Images = new BucketOptions
					{
						Name = "images", PublicBaseUrl = "https://images.example.com"
					},
					Uploads = new BucketOptions
					{
						Name = "uploads", PublicBaseUrl = "https://images.example.com"
					},
					Documents = new BucketOptions
					{
						Name = "documents", PublicBaseUrl = "https://images.example.com"
					}
				}));

		services.ConfigureDbContext<UnifiedDbContext>(options =>
			options.UseNpgsql(pgsql => pgsql.MigrationsAssembly("Main.Migrator")));

		var pgsqlBuilder = new NpgsqlConnectionStringBuilder(args.PgsqlConnectionString);

		services.AddSingleton(
			Options.Create(
				new DatabaseOptions
				{
					Host = pgsqlBuilder.Host!,
					Database = pgsqlBuilder.Database!,
					Username = pgsqlBuilder.Username!,
					Password = pgsqlBuilder.Password!,
					Port = pgsqlBuilder.Port
				}));

		services.AddSingleton(
			Options.Create(
				new SecretEncryptionOptions
				{
					Secret = "some secret"
				}));
		services.AddSingleton(
			Options.Create(
				new JwtOptions
				{
					ValidDuration = TimeSpan.FromMinutes(15),
					ValidIssuer = "main-tests",
					IssuerSigningKey = "main-tests-signing-key-at-least-32-characters"
				}));

		services.AddSingleton<S3StorageServiceStub>();
		services.AddSingleton<IS3Service>(sp => sp.GetRequiredService<S3StorageServiceStub>());
		services.AddProjectJsonSerialization();

		services
			.AddJsonSigner()
			.AddCacheLayer("test")
			.AddApplicationCache()
			.AddFullSecurityLayer(passwordRules)
			.AddCommonLayer();

		services.RemoveAll<IUserContext>();
		services.AddScoped<IUserContext, UserContextMock>();
		services.AddSystemOptionsForTests();

		services.AddSingleton<PublishEndpointAccessor>();
		services.AddScoped<IPublishEndpoint>(sp
			=> sp.GetRequiredService<PublishEndpointAccessor>().Publisher);
		services.RemoveAll<IFusionCacheBackplane>();
		services.AddSingleton<IFusionCacheBackplane, FusionCacheBackplaneStub>();

		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider;
	}

	private static void RegisterGlobalBasicContexts()
	{
		TestBase.RegisterGlobalBasicContext<LocalizedTestContext>();
		TestBase.RegisterGlobalBasicContext<RolesTestContext>();
		TestBase.RegisterGlobalBasicContext<UserContextTestContext>();
	}
}
