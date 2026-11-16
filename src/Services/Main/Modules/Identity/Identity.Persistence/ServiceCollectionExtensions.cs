using Persistence.Common;
using Main.Application.Interfaces.Persistence;
using Main.Persistence.Repositories.User;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Identity.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityPersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<IdentityDbContextModule>().AddEntities(typeof(Main.Entities.User.User).Assembly);
		services.AddScoped<IUserRepository, UserRepository>();

		return services;
	}
}
