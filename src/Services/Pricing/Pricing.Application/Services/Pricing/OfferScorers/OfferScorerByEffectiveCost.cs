using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing.OfferScorers;

public sealed class OfferScorerByEffectiveCost(
    ISettingsService settingsService) : IOfferScorer
{
    public async ValueTask<decimal> GetScoreAsync(
        OfferScoreContext context,
        CancellationToken cancellationToken = default)
    {
        var penaltyPerDay = await GetDeliveryDayPenaltyAsync(cancellationToken);

        var effectiveCost = context.Cost + GetAverageDeliveryPenalty(context, penaltyPerDay);

        return ToScore(effectiveCost);
    }

    private async ValueTask<decimal> GetDeliveryDayPenaltyAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetOrDefault<PricingSetting>(cancellationToken);

        return settings.Data.DeliveryDayPenalty;
    }

    private static decimal GetAverageDeliveryPenalty(
        OfferScoreContext context,
        decimal penaltyPerDay)
    {
        var averageDeliveryDays = (context.DeliveryDays + context.GuaranteedDeliveryDays) / 2m;

        return averageDeliveryDays * penaltyPerDay;
    }

    private static decimal ToScore(decimal effectiveCost)
    {
        return -effectiveCost;
    }
}