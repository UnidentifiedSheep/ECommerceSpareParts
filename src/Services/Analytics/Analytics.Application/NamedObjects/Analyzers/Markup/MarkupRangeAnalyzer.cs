using Analytics.Entities;
using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.NamedObjects.Analyzers.Markup;

public class MarkupRangeAnalyzer(
    IReadRepository<SaleContent, int> repository
) : MarkupAnalyzerNamedObjectBase
{
    private const int BatchSize = 1000;
    private const double MaxStdDev = 0.08;
    private const decimal MaxCostRatio = 1.5m;
    private const decimal MinAllowedMarkup = 0.01m;
    private const decimal MaxAllowedMarkup = 3m;

    public const string AnalyzerSystemName = nameof(MarkupRangeAnalyzer);

    public override string NameLocalizationKey => "markup.range.analyzer.name";
    public override string DescriptionLocalizationKey => "markup.range.analyzer.description";
    public override string SystemName => AnalyzerSystemName;

    public override async Task<IReadOnlyList<MarkupRangeDraft>> AnalyzeAsync(
        MarkupAnalyzerInput input,
        CancellationToken cancellationToken = default)
    {
        var ranges = new List<MarkupRangeDraft>();
        var currentBucket = new MarkupBucketBuilder();
        MarkupRangeCursor? cursor = null;

        while (true)
        {
            var result = await LoadBatch(
                input,
                cursor,
                cancellationToken);

            if (result.Count == 0) break;

            foreach (var row in result)
                TryAddToRanges(
                    row,
                    ranges,
                    ref currentBucket);

            cursor = MarkupRangeCursor.From(result[^1]);
        }

        if (currentBucket.Count > 0) ranges.Add(currentBucket.Build());

        return ranges;
    }

    private async Task<List<SaleContentMarkupRow>> LoadBatch(
        MarkupAnalyzerInput input,
        MarkupRangeCursor? cursor,
        CancellationToken cancellationToken)
    {
        var query = BuildRowsQuery(input);

        if (cursor is not null)
            query = query.Where(x =>
                x.AvgBuyPriceBase > cursor.AvgBuyPriceBase ||
                x.AvgBuyPriceBase == cursor.AvgBuyPriceBase && x.Id > cursor.Id);

        return await query
            .OrderBy(x => x.AvgBuyPriceBase)
            .ThenBy(x => x.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<SaleContentMarkupRow> BuildRowsQuery(MarkupAnalyzerInput input)
    {
        var query = repository.Query
            .AsNoTracking()
            .Where(x => x.Details.Any());

        if (input.StartDate is not null) query = query.Where(x => x.Sale.CreatedAt >= input.StartDate);

        if (input.EndDate is not null) query = query.Where(x => x.Sale.CreatedAt <= input.EndDate);

        return query.Select(x => new SaleContentMarkupRow
        {
            Id = x.Id,
            SaleUnitPriceBase = x.PriceInBaseCurrency / (1 - x.Discount),
            AvgBuyPriceBase =
                x.Details.Sum(d => d.BuyPriceInBaseCurrency * d.Count) /
                x.Details.Sum(d => d.Count)
        });
    }

    private static void TryAddToRanges(
        SaleContentMarkupRow row,
        List<MarkupRangeDraft> ranges,
        ref MarkupBucketBuilder currentBucket)
    {
        if (!TryCalculateMarkup(row, out var markup)) return;

        if (currentBucket.Count == 0)
        {
            currentBucket.Add(row.AvgBuyPriceBase, markup);
            return;
        }

        var testBucket = currentBucket.Copy();
        testBucket.Add(row.AvgBuyPriceBase, markup);

        if (!ShouldStartNewBucket(
                row,
                currentBucket,
                testBucket) || currentBucket.Count <= 1)
        {
            currentBucket = testBucket;
            return;
        }

        ranges.Add(currentBucket.Build());

        currentBucket = new MarkupBucketBuilder();
        currentBucket.Add(row.AvgBuyPriceBase, markup);
    }

    private static bool TryCalculateMarkup(
        SaleContentMarkupRow row,
        out decimal markup)
    {
        markup = 0;

        if (row.AvgBuyPriceBase <= 0) return false;

        markup = (row.SaleUnitPriceBase - row.AvgBuyPriceBase) / row.AvgBuyPriceBase;
        return markup is >= MinAllowedMarkup and <= MaxAllowedMarkup;
    }

    private static bool ShouldStartNewBucket(
        SaleContentMarkupRow row,
        MarkupBucketBuilder currentBucket,
        MarkupBucketBuilder testBucket)
    {
        var costRatioExceeded =
            currentBucket.FromCost > 0 &&
            row.AvgBuyPriceBase / currentBucket.FromCost > MaxCostRatio;

        var stdDevExceeded =
            testBucket is { Count: > 1, StdDev: > MaxStdDev };

        return stdDevExceeded || costRatioExceeded;
    }

    private sealed class SaleContentMarkupRow
    {
        public int Id { get; init; }
        public decimal SaleUnitPriceBase { get; init; }
        public decimal AvgBuyPriceBase { get; init; }
    }

    private sealed record MarkupRangeCursor(int Id, decimal AvgBuyPriceBase)
    {
        public static MarkupRangeCursor From(SaleContentMarkupRow row)
        {
            return new MarkupRangeCursor(row.Id, row.AvgBuyPriceBase);
        }
    }
}