using Application.Common.Models;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;

namespace Pricing.Application.Models.Jobs;

public class InvalidateStalePriceOptionsJob
{
    public static UniqJobItem Create(int maxAttempts = 3)
    {
        var naturalKey = BuildNaturalKey();

        return new UniqJobItem(
            InvalidateStalePriceOptionsLrt.LrtName,
            "{}",
            maxAttempts,
            naturalKey);
    }

    private static string BuildNaturalKey()
    {
        return $"{InvalidateStalePriceOptionsLrt.LrtName}";
    }
}
