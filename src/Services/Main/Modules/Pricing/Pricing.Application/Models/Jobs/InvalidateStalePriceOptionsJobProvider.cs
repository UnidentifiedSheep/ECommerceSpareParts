using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;

namespace Pricing.Application.Models.Jobs;

[Lifetime(Lifetime.Singleton)]
public sealed class
	InvalidateStalePriceOptionsJobProvider : IJobProvider<InvalidateStalePriceOptionsLrt, NoneInputState>
{
	public Job Create(NoneInputState _, int maxAttempts = 3)
	{
		var naturalKey = BuildNaturalKey();

		return SingleRunJob.CreateUnique(
			naturalKey,
			InvalidateStalePriceOptionsLrt.LrtName,
			NoneInputState.Json,
			maxAttempts);
	}

	private static string BuildNaturalKey() => $"{InvalidateStalePriceOptionsLrt.LrtName}";
}
