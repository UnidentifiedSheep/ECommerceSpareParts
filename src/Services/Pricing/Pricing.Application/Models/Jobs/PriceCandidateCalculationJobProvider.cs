using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Domain.CommonEntities.Job;
using Pricing.Application.Lrts.PriceCandidateCalculation;

namespace Pricing.Application.Models.Jobs;

[Lifetime(Lifetime.Singleton)]
public sealed class PriceCandidateCalculationJobProvider
    : IJobProvider<PriceCandidateCalculationLrt, PriceCandidateCalculationState>
{
    public Job Create(
        PriceCandidateCalculationState inputState,
        int maxAttempts = 3)
    {
        var naturalKey = BuildNaturalKey(
            inputState.ProductId,
            inputState.StorageName);

        return SingleRunJob.CreateUnique(
            naturalKey,
            PriceCandidateCalculationLrt.LrtName,
            JsonSerializer.Serialize(inputState),
            maxAttempts);
    }

    private static string BuildNaturalKey(int productId, string storageName)
    {
        return $"{PriceCandidateCalculationLrt.LrtName}:{productId}:{storageName}";
    }
}
