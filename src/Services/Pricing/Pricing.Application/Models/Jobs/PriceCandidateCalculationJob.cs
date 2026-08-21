using System.Text.Json;
using Application.Common.Models;
using Pricing.Application.Lrts.PriceCandidateCalculation;

namespace Pricing.Application.Models.Jobs;

public static class PriceCandidateCalculationJob
{
    public static UniqJobItem Create(
        int productId,
        string storageName,
        int maxAttempts = 3)
    {
        var state = new PriceCandidateCalculationState
        {
            ProductId = productId,
            StorageName = storageName
        };

        var naturalKey = BuildNaturalKey(productId, storageName);

        return new UniqJobItem(
            PriceCandidateCalculationLrt.LrtName,
            JsonSerializer.Serialize(state),
            maxAttempts,
            naturalKey);
    }

    private static string BuildNaturalKey(int productId, string storageName)
    {
        return $"{PriceCandidateCalculationLrt.LrtName}:{productId}:{storageName}";
    }
}
