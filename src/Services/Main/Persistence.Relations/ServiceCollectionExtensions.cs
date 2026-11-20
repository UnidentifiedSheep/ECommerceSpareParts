using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using Persistence.Relations.Configs;

namespace Persistence.Relations;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRelationsPersistence(this IServiceCollection services)
	{
		RelationsSortByConfig.Configure();
		return services.AddDbContextModule<RelationsDbContextModule>();
	}
}
