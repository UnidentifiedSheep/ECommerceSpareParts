using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Core.Models;
using Exceptions.Exceptions.Markups;
using MediatR;
using Events_MarkupRangesUpdatedEvent = Main.Application.Events.MarkupRangesUpdatedEvent;
using MarkupRangesUpdatedEvent = Main.Application.Events.MarkupRangesUpdatedEvent;

namespace Main.Application.Pricing;

public class MarkupGenerator(
    IDefaultSettingsRepository defaultSettingsRepository,
    IBuySellPriceRepository buySellPriceRepository,
    ICurrencyRepository currencyRepository,
    ICurrencyConverter currencyConverter,
    IMarkupRepository markupRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator) : IMarkupGenerator
{
    private DefaultSettings _defaultSetting = null!;

    public async Task ReCalculateMarkupAsync(CancellationToken cancellationToken = default)
    {
        _defaultSetting = await defaultSettingsRepository.GetDefaultSettingsAsync(cancellationToken);
        await ConvertBuySellToDefaultCurrencyAsync(cancellationToken);
        await CalculateNullMarkUpsAsync(cancellationToken);
        await MarkOutliersAsync(cancellationToken);
        await UpdateMarkUpRangesAsync(cancellationToken);
        await mediator.Publish(new Events_MarkupRangesUpdatedEvent(), cancellationToken);
    }

    private async Task ConvertBuySellToDefaultCurrencyAsync(CancellationToken cancellationToken)
    {
        var defaultCurrency =
            await currencyRepository.GetCurrencyById(_defaultSetting.DefaultCurrency, false, cancellationToken);
        if (defaultCurrency == null)
            throw new ArgumentNullException(nameof(defaultCurrency), "Не удалось найти стандартную валюту");
        var buffer = new List<BuySellPrice>();
        const int bufferSize = 10_000;

        await foreach (var price in buySellPriceRepository.GetBuySellPrices(
                           x => x.CurrencyId != defaultCurrency.Id, false, cancellationToken))
        {
            price.BuyPrice =
                Math.Round(currencyConverter.ConvertTo(price.BuyPrice, price.CurrencyId, defaultCurrency.Id), 2);
            price.SellPrice =
                Math.Round(currencyConverter.ConvertTo(price.SellPrice, price.CurrencyId, defaultCurrency.Id), 2);
            price.CurrencyId = defaultCurrency.Id;
            buffer.Add(price);

            if (buffer.Count < bufferSize) continue;

            await buySellPriceRepository.UpdateRange(buffer, cancellationToken);
            buffer.Clear();
        }

        if (buffer.Count > 0)
            await buySellPriceRepository.UpdateRange(buffer, cancellationToken);
    }

    private async Task CalculateNullMarkUpsAsync(CancellationToken cancellationToken)
    {
        var buffer = new List<BuySellPrice>();
        const int bufferSize = 10_000;

        await foreach (var price in buySellPriceRepository.GetBuySellPrices(
                           x => x.Markup == null, false, cancellationToken))
        {
            price.Markup = Math.Round((price.SellPrice - price.BuyPrice) / price.BuyPrice * 100, 2);
            buffer.Add(price);

            if (buffer.Count >= bufferSize)
            {
                await buySellPriceRepository.UpdateRange(buffer, cancellationToken);
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
            await buySellPriceRepository.UpdateRange(buffer, cancellationToken);
    }

    private async Task MarkOutliersAsync(CancellationToken cancellationToken)
    {
        var maxBuyPrice = await buySellPriceRepository.GetMaxBuyPrice(cancellationToken) ?? 0;
        const decimal step = 1.5m;
        const int maxEmptyBuckets = 100;
        var emptyCount = 0;
        var buffer = new List<BuySellPrice>();
        const int bufferSize = 10_000;

        // Проходим по диапазонам buyPrice от 0.11 до maxBuyPrice
        for (var i = 0.11m; i <= maxBuyPrice;)
        {
            var next = Math.Round(i * step, 2);
            var rangeItems = await buySellPriceRepository
                .GetBuySellPricesAsList(x => x.BuyPrice >= i && x.BuyPrice <= next, false, cancellationToken);

            if (rangeItems.Count == 0)
            {
                emptyCount++;
                i = next + 0.01m;
                if (emptyCount > maxEmptyBuckets)
                    break;
                continue;
            }

            var markups = rangeItems.Select(x => x.Markup ?? 0).ToList();
            var mean = markups.Average();
            var stdDev = CalculateStandardDeviation(markups, mean);
            var lowerBound = mean - 3 * stdDev;
            var upperBound = mean + 3 * stdDev;

            foreach (var item in rangeItems)
            {
                var markup = item.Markup ?? 0;
                // Если значение выбрасывается за пределы статистической границы или меньше минимального маркапа
                if (markup < lowerBound || markup > upperBound || markup <= _defaultSetting.MinimalMarkup)
                {
                    item.IsOutLiner = true;
                    buffer.Add(item);

                    if (buffer.Count >= bufferSize)
                    {
                        await buySellPriceRepository.UpdateRange(buffer, cancellationToken);
                        buffer.Clear();
                    }
                }
            }

            i = next + 0.01m;
        }

        if (buffer.Count > 0)
            await buySellPriceRepository.UpdateRange(buffer, cancellationToken);
    }

    private async Task UpdateMarkUpRangesAsync(CancellationToken cancellationToken)
    {
        var generatedMarkup = await markupRepository.GetGeneratedMarkupsAsync(true, cancellationToken)
                              ?? throw new MarkupGroupNotFoundException(-1);
        unitOfWork.Remove(generatedMarkup);

        var group = new MarkupGroup
        {
            CurrencyId = _defaultSetting.DefaultCurrency,
            IsAutoGenerated = true,
            Name = "Auto Generated Markup For Default Currency"
        };

        var firstCent = await buySellPriceRepository
            .GetBuySellPricesAsList(x => x.BuyPrice >= 0 && x.BuyPrice <= 0.1m, false, cancellationToken);
        if (firstCent.Any())
        {
            var avg = firstCent.Average(x => x.Markup) ?? 25;
            var markup = avg > 50 ? avg - 10 : avg;
            group.MarkupRanges.Add(new MarkupRange
            {
                RangeStart = 0,
                RangeEnd = 0.1m,
                Markup = Math.Round(markup, 2),
                Group = group
            });
        }

        var maxBuyPrice = await buySellPriceRepository.GetMaxBuyPrice(cancellationToken) ?? 0;
        const decimal step = 1.5m;
        var stopCount = 0;

        for (var i = 0.11m; i <= maxBuyPrice;)
        {
            var next = Math.Round(i * step, 2);
            var bucket = await buySellPriceRepository
                .GetBuySellPricesAsList(x => x.BuyPrice >= i && x.BuyPrice <= next && !x.IsOutLiner, false,
                    cancellationToken);
            if (bucket.Count == 0)
            {
                stopCount++;
                i = next + 0.01m;
                if (stopCount > 100)
                    break;
                continue;
            }

            var avg = bucket.Average(x => x.Markup) ?? 25;
            var markup = avg > 50 ? avg - 10 : avg;
            group.MarkupRanges.Add(new MarkupRange
            {
                RangeStart = i,
                RangeEnd = next,
                Markup = Math.Round(markup, 2),
                Group = group
            });
            i = next + 0.01m;
        }

        var orderedRanges = group.MarkupRanges.OrderBy(x => x.RangeStart).ToList();
        if (orderedRanges.Count > 1)
        {
            orderedRanges[0].RangeStart = 0;
            orderedRanges[0].RangeEnd = orderedRanges[1].RangeStart - 0.01m;
        }

        await unitOfWork.AddAsync(group, cancellationToken);
    }

    private static decimal CalculateStandardDeviation(List<decimal> values, decimal mean)
    {
        var variance = values.Average(v => Math.Pow((double)(v - mean), 2));
        return (decimal)Math.Round(Math.Sqrt(variance), 2);
    }
}