using Application.Common.Models.Lrt;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class LrtQuotaServiceCollectionExtensions
{
	public static IServiceCollection AddLrtQuota(
		this IServiceCollection services,
		string systemName,
		int maxParallelPerWorker)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxParallelPerWorker);

		return services.AddLrtQuota(systemName, _ => maxParallelPerWorker);
	}

	public static IServiceCollection AddLrtQuota(
		this IServiceCollection services,
		string systemName,
		Func<IServiceProvider, int> resolveMaxParallelPerWorker)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(systemName);
		ArgumentNullException.ThrowIfNull(resolveMaxParallelPerWorker);

		services.AddSingleton(new LrtQuotaRegistration(systemName, resolveMaxParallelPerWorker));
		return services;
	}
}
