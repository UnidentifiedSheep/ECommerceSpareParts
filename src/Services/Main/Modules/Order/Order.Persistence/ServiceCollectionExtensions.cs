using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Order.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrderPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<OrderDbContextModule>().AddEntities(typeof(Entities.Order.Order.Order).Assembly);
}
