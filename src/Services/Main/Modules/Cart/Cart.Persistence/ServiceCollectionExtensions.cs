using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Cart.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCartPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<CartDbContextModule>().AddEntities(typeof(Main.Entities.Cart.Cart).Assembly);
}
