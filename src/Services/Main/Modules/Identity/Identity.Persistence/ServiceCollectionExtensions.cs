using Main.Entities.User;
using Main.Modules.Identity.Application.Interfaces.Persistence;
using Main.Modules.Identity.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Identity.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityPersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<IdentityDbContextModule>().AddEntities(typeof(User).Assembly);
		services.AddScoped<IUserRepository, UserRepository>();

		return services;
	}
}
