using Main.Application.Interfaces.Persistence;
using Main.Persistence.Repositories.User;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Identity.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityPersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<IdentityDbContextModule>();
		services.AddScoped<IUserRepository, UserRepository>();

		return services;
	}
}
