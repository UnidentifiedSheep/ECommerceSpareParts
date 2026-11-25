using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Interfaces.Persistence;
using Main.Modules.Logistics.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Logistics.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddLogisticsPersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<LogisticsDbContextModule>().AddEntities(typeof(DeliverySchedule).Assembly);

		services.AddScoped<IDeliveryDailyStatRepository, DeliveryDailyStatRepository>();

		return services;
	}
}
