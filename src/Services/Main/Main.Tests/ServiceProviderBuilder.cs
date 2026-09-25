using System.Globalization;
using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common;
using Application.Common.Models.Options.S3;
using Cache;
using Locan.Hosting;
using Main.Application.Configs;
using Main.Application.Models;
using Main.Cache;
using Main.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Persistence;
using Security;
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
					}
				}));

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

		services.AddScoped<S3StorageServiceStub>();
		services.AddScoped<IS3StorageService>(sp => sp.GetRequiredService<S3StorageServiceStub>());
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

		services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
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
