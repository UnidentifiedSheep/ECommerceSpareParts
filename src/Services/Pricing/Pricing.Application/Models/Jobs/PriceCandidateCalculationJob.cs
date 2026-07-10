using System.Text.Json;
using Application.Common.Handlers.Jobs;
using Pricing.Application.Lrts.PriceCandidateCalculationLrt;

namespace Pricing.Application.Models.Jobs;

public static class PriceCandidateCalculationJob
{
    public static TryEnqueueUniqJobItem Create(
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

        return new TryEnqueueUniqJobItem(
            naturalKey,
            PriceCandidateCalculationLrt.LrtName,
            JsonSerializer.Serialize(state),
            maxAttempts);
    }

    private static string BuildNaturalKey(int productId, string storageName)
    {
        return $"{PriceCandidateCalculationLrt.LrtName}:{productId}:{storageName}";
    }
}