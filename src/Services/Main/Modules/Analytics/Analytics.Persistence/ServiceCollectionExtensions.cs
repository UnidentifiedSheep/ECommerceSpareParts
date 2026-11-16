using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Analytics.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAnalyticsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<AnalyticsDbContextModule>().AddEntities(typeof(Main.Entities.Analytics.SaleContentAnalytics).Assembly);
}
