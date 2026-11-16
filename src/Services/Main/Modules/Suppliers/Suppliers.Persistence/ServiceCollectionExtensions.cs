using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Suppliers.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSuppliersPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<SuppliersDbContextModule>();
}
