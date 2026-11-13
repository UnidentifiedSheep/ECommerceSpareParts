using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Product.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddProductPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<ProductDbContextModule>();
}
