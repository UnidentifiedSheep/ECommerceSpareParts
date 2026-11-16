using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Persistence.Relations;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRelationsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<RelationsDbContextModule>();
}
