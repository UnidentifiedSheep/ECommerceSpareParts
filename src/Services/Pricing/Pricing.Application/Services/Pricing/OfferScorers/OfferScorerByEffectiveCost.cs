using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing.OfferScorers;

public sealed class OfferScorerByEffectiveCost(
    ISettingsService settingsService) : IOfferScorer
{
    public async ValueTask<decimal> GetCostScoreAsync(
        OfferCostScoreContext context,
        CancellationToken cancellationToken = default)
    {
        var penaltyPerDay = await GetDeliveryDayPenaltyAsync(cancellationToken);

        var effectiveCost = context.Cost + GetAverageDeliveryPenalty(
            context.DeliveryDays,
            context.GuaranteedDeliveryDays, 
            penaltyPerDay);

        return ToScore(effectiveCost);
    }
    public async ValueTask<IReadOnlyList<CalculatedScoredPriceCandidate>> GetResultingScoreAsync(
        IReadOnlyList<CalculatedPriceCandidate> candidates, 
        CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0) return [];

        var penaltyPerDay = await GetDeliveryDayPenaltyAsync(cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var effect = candidate.CostInBaseCurrency + GetAverageDeliveryPenalty(
                    (int)Math.Ceiling(candidate.DeliveryTime.TotalDays),
                    (int)Math.Ceiling(candidate.GuaranteedDeliveryTime.TotalDays),
                    penaltyPerDay);
                return CalculatedScoredPriceCandidate.From(candidate, ToScore(effect));
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.CostInBaseCurrency)
            .ThenBy(x => x.DeliveryTime)
            .ThenBy(x => x.GuaranteedDeliveryTime)
            .ThenByDescending(x => x.AvailableQuantity)
            .ToList();
    }

    private async ValueTask<decimal> GetDeliveryDayPenaltyAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetOrDefault<PricingSetting>(cancellationToken);

        return settings.Data.DeliveryDayPenalty;
    }

    private static decimal GetAverageDeliveryPenalty(
        int deliveryDays,
        int guaranteedDeliveryDays,
        decimal penaltyPerDay)
    {
        var averageDeliveryDays = (deliveryDays + guaranteedDeliveryDays) / 2m;

        return averageDeliveryDays * penaltyPerDay;
    }
    
    private static decimal ToScore(decimal effectiveCost)
    {
        return effectiveCost <= 0
            ? 0
            : 1m / effectiveCost;
    }
}