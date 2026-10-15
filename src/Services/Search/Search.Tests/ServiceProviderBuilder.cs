using Cache;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.Abstractions.Options;
using Search.Application.Consumers.Product;
using Tests.Extensions;
using Tests.Interfaces.ServiceProvider;
using Tests.Stubs;
using ApplicationServices = Search.Application.ServiceProvider;
using PersistenceServices = Search.Persistence.ServiceProvider;

namespace Search.Tests;

public sealed class ServiceProviderBuilder : IServiceProviderBuilder<ServiceProviderArguments>
{
	public IServiceProvider Build(ServiceProviderArguments args)
	{
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
		{
			[$"{OpenSearchOptions.SectionName}:Uri"] = args.OpenSearchEndpoint.ToString(),
			[$"{OpenSearchOptions.SectionName}:IndexOptions:Products"] = args.IndexPrefix + "products",
			[$"{OpenSearchOptions.SectionName}:IndexOptions:Producers"] = args.IndexPrefix + "producers",
			[$"{OpenSearchOptions.SectionName}:IndexOptions:CatalogueCandidates"] = args.IndexPrefix + "catalogue-candidates",
			[$"{RedisOptions.SectionName}:Url"] = args.RedisConnectionString
		}).Build();
		var services = new ServiceCollection();
		services.RegisterTestContexts();
		services.AddLogging();
		services.AddOptions<OpenSearchOptions>().Bind(configuration.GetSection(OpenSearchOptions.SectionName));
		services.AddOptions<RedisOptions>().Bind(configuration.GetSection(RedisOptions.SectionName));
		ApplicationServices.AddApplicationLayer(services, configuration);
		PersistenceServices.AddPersistenceLayer(services);
		services.AddCacheLayer("search-tests");
		services.AddSystemOptionsForTests();
		services.AddTransient<IPublishEndpoint, MessageBrokerStub>();
		services.AddTransient<ProductUpdatedConsumer>();
		services.AddTransient<ProductCharacteristicsUpdatedConsumer>();
		services.AddTransient<ProductDimensionsUpdatedConsumer>();
		services.AddTransient<ProductWeightUpdatedConsumer>();
		return services.BuildServiceProvider();
	}
}
