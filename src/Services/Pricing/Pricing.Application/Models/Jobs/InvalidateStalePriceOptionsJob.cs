using Application.Common.Handlers.Jobs;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;

namespace Pricing.Application.Models.Jobs;

public class InvalidateStalePriceOptionsJob
{
    public static TryEnqueueUniqJobItem Create(int maxAttempts = 3)
    {
        var naturalKey = BuildNaturalKey();

        return new TryEnqueueUniqJobItem(
            naturalKey,
            InvalidateStalePriceOptionsLrt.LrtName,
            "{}",
            maxAttempts);
    }

    private static string BuildNaturalKey()
    {
        return $"{InvalidateStalePriceOptionsLrt.LrtName}";
    }
}