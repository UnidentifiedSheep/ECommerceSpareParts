using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing.OfferScorers;

public sealed class OfferScorerByEffectiveCost(
    ICurrencyConverter currencyConverter,
    ISettingsService settingsService) : IOfferScorer
{
    public async ValueTask<decimal> GetCostScoreAsync(
        OfferCostScoreContext context,
        CancellationToken cancellationToken = default)
    {
        var penaltyPerDay = await GetDeliveryDayPenaltyAsync(cancellationToken);

        var effectiveCost = context.CostInBase + GetAverageDeliveryPenalty(
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
        var scored = new List<(CalculatedScoredPriceCandidate candidate, decimal basePrice)>();

        foreach (var candidate in candidates)
        {
            var costInBaseCurrency = await currencyConverter.ConvertToBaseAsync(
                candidate.Cost,
                candidate.CurrencyId,
                cancellationToken);
            var effect = costInBaseCurrency + GetAverageDeliveryPenalty(
                (int)Math.Ceiling(candidate.DeliveryTime.TotalDays),
                (int)Math.Ceiling(candidate.GuaranteedDeliveryTime.TotalDays),
                penaltyPerDay);
            scored.Add((CalculatedScoredPriceCandidate.From(candidate, ToScore(effect)), costInBaseCurrency));
        }
        
        return scored
            .OrderByDescending(x => x.candidate.Score)
            .ThenBy(x => x.basePrice)
            .ThenBy(x => x.candidate.DeliveryTime)
            .ThenBy(x => x.candidate.GuaranteedDeliveryTime)
            .ThenByDescending(x => x.candidate.AvailableQuantity)
            .Select(x => x.candidate)
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