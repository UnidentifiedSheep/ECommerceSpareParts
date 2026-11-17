using Application.Common.Extensions;
using Main.Modules.Jobs.Application.Models.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Main.Modules.Jobs.Application.LRT;

public static class LrtQuotaRegistrations
{
	public const string Default = "default";

	public static IServiceCollection AddDefaultLrtQuotas(this IServiceCollection services)
		=> services.AddLrtQuota(
			Default,
			provider => provider.GetRequiredService<IOptions<LrtExecutorOptions>>()
				.Value.MaxParallelPerWorker);
}
