using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Core.Models;
using Analytics.Core.Static;
using Application.Common.Interfaces;
using Contracts.Markup;
using Contracts.Models.Markup;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using MediatR;

namespace Analytics.Application.Handlers.AnalyzeSalesForMarkup;

[Transactional]
public record AnalyzeSalesForMarkup : ICommand;

public class AnalyzeSalesForMarkupHandler(
    ISellInfoRepository sellInfoRepository,
    ICurrencyConverter currencyConverter,
    IUnitOfWork unitOfWork,
    IMessageBroker messageBroker) : ICommandHandler<AnalyzeSalesForMarkup>
{
    private const decimal Step = 1.5m;
    private const int ConvertBatchSize = 10_000;
    private const double OutlierMultiplier = 3.0;

    public async Task<Unit> Handle(AnalyzeSalesForMarkup request, CancellationToken cancellationToken)
    {
        var minDate = DateTime.Now.AddYears(-2).Date;
        
        await ConvertToDefaultCurrency(minDate, cancellationToken);
        
        var maxBuyPrice = (await sellInfoRepository.GetWithMaximumBuyPrice(false, cancellationToken))?
            .BuyPrices ?? 0m;
        
        var rangeStats = await BuildRangeStatsStreaming(maxBuyPrice, minDate, cancellationToken);
        
        var finalMarkups = await BuildFinalMarkupsStreaming(rangeStats, minDate, cancellationToken);
        
        await messageBroker.Publish(new MarkupGroupGeneratedEvent(finalMarkups, Global.DefaultCurrencyId), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task ConvertToDefaultCurrency(DateTime minDate, CancellationToken cancellationToken = default)
    {
        int counter = 0;
        await foreach (var item in sellInfoRepository
                           .GetSellInfos(x =>
                                   (x.BuyCurrencyId != Global.DefaultCurrencyId ||
                                    x.SellCurrencyId != Global.DefaultCurrencyId)
                                   && x.SellDate >= minDate)
                           .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            item.SellPrice = currencyConverter.ConvertTo(item.SellPrice, item.SellCurrencyId, Global.DefaultCurrencyId);
            item.BuyPrices = currencyConverter.ConvertTo(item.BuyPrices, item.BuyCurrencyId, Global.DefaultCurrencyId);

            counter++;
            if (counter < ConvertBatchSize) continue;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            counter = 0;
        }

        if (counter > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    private async Task<List<MarkupRangeStats>> BuildRangeStatsStreaming(decimal maxBuyPrice, DateTime minDate,
        CancellationToken cancellationToken = default)
    {
        var ranges = new List<(decimal From, decimal To)>();

        for (decimal i = 0m; i <= maxBuyPrice;)
        {
            var next = Math.Round(i * Step, 2);
            if (i == 0m) next = 0.1m;
            
            ranges.Add((i, next));

            i = i == 0m ? 0.11m : next + 0.01m; 
        }
        
        var accumulators = new Dictionary<(decimal From, decimal To), WelfordAccumulator>();
        foreach (var r in ranges)
            accumulators[r] = new WelfordAccumulator();

        // Проходимся один раз по всем записям (стрим)
        await foreach (var item in sellInfoRepository
                           .GetSellInfos(x => x.SellDate >= minDate)
                           .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var markup = item.Markup ?? Calculate.Markup(item.BuyPrices, item.SellPrice);

            foreach (var r in ranges)
            {
                if (item.BuyPrices < r.From || item.BuyPrices > r.To) continue;
                accumulators[r].Add((double)markup);
                break;
            }
        }
        
        var results = new List<MarkupRangeStats>(accumulators.Count);
        foreach (var kv in accumulators)
        {
            var acc = kv.Value;
            if (acc.Count == 0)
                continue;

            double variance = acc.VariancePopulation;
            if (variance < 0) variance = 0; 

            var mean = (decimal)acc.Mean;
            var stdDev = Math.Round((decimal)Math.Sqrt(variance), 2);

            results.Add(new MarkupRangeStats
            {
                From = kv.Key.From,
                To = kv.Key.To,
                Mean = Math.Round(mean, 2),
                StdDev = stdDev,
                Count = acc.Count
            });
        }

        return results;
    }
    private async Task<List<MarkupRangeStat>> BuildFinalMarkupsStreaming(List<MarkupRangeStats> stats, DateTime minDate,
        CancellationToken cancellationToken = default)
    {
        var accumFiltered = stats.ToDictionary(
            s => (s.From, s.To),
            s => new FilterAccumulator { Sum = 0m, Count = 0, MeanIfAllFiltered = s.Mean });
        
        var boundaries = stats.ToDictionary(
            s => (s.From, s.To),
            s =>
            {
                var lower = s.Mean - (decimal)OutlierMultiplier * s.StdDev;
                var upper = s.Mean + (decimal)OutlierMultiplier * s.StdDev;
                return (Lower: lower, Upper: upper);
            });

        await foreach (var item in sellInfoRepository
                           .GetSellInfos(x => x.SellDate >= minDate)
                           .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var markup = item.Markup ?? Calculate.Markup(item.BuyPrices, item.SellPrice);
            
            foreach (var stat in stats)
            {
                if (item.BuyPrices < stat.From || item.BuyPrices > stat.To) continue;
                var key = (stat.From, stat.To);
                var (lower, upper) = boundaries[key];
                
                if (markup >= lower && markup <= upper)
                {
                    var acc = accumFiltered[key];
                    acc.Sum += markup;
                    acc.Count++;
                }

                break;
            }
        }
        
        var results = new List<MarkupRangeStat>(stats.Count);
        foreach (var s in stats)
        {
            var key = (s.From, s.To);
            var acc = accumFiltered[key];
            decimal finalMean = acc.Count == 0 ? Math.Round(s.Mean, 2) : Math.Round(acc.Sum / acc.Count, 2);
            results.Add(new MarkupRangeStat(s.From, s.To, finalMean));
        }

        return results;
    }
}