using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Organization.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrganizationPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<OrganizationDbContextModule>().AddEntities(typeof(Main.Entities.Organization.Organization).Assembly);
}
