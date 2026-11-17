using Main.Modules.Notifications.Persistence.Delivery.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notification.Core.Interfaces.Repositories;
using Persistence.Interfaces;
using Persistence.Services;

namespace Main.Modules.Notifications.Persistence.Delivery;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationPersistence<TContext>(this IServiceCollection services)
		where TContext : DbContext
	{
		services.TryAddScoped<IContextMetadata, ContextMetadata<TContext>>();
		services.TryAddScoped<IQueryableExtensions, QueryableExtensions>();
		services.TryAddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository<TContext>>();

		return services;
	}
}
