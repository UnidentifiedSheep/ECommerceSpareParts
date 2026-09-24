using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notification.Core.Interfaces.Repositories;
using Notification.Persistence.Interfaces;
using Notification.Persistence.Repositories;
using Persistence.Interfaces;
using Persistence.Services;

namespace Notification.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationPersistence<TContext>(this IServiceCollection services)
		where TContext : DbContext, INotificationDbContext
	{
		services.TryAddScoped<INotificationDbContext>(provider =>
			provider.GetRequiredService<TContext>());
		services.TryAddScoped<IContextMetadata, ContextMetadata<TContext>>();
		services.TryAddScoped<IQueryableExtensions, QueryableExtensions>();
		services.TryAddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();

		return services;
	}
}
