using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Cart.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCartPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<CartDbContextModule>();
}
