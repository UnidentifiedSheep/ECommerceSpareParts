using Application.Common.Extensions;
using Application.Common.Models.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Application.Common.LRT;

public static class LrtQuotaRegistrations
{
	public const string Default = "default";

	public static IServiceCollection AddDefaultLrtQuotas(this IServiceCollection services)
		=> services.AddLrtQuota(
			Default,
			provider => provider.GetRequiredService<IOptions<LrtExecutorOptions>>().Value.MaxParallelPerWorker);
}
