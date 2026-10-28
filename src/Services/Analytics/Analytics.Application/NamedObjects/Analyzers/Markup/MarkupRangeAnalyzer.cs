using Analytics.Application.Extensions;
using Analytics.Entities;
using Application.Common.Interfaces.Repositories;
using Locan.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.NamedObjects.Analyzers.Markup;

public class MarkupRangeAnalyzer(IReadRepository<SaleContent, int> repository) : MarkupAnalyzerNamedObjectBase
{
	private const int BatchSize = 1000;

	private const int DecimalScale = 10;

	private const int MinBucketSampleSize = 100;

	private const double MaxStdDev = 0.08;

	private const decimal MaxCostRatio = 2m;

	private const decimal MinAllowedMarkup = 0.01m;

	private const decimal MaxAllowedMarkup = 3m;

	public const string AnalyzerSystemName = nameof(MarkupRangeAnalyzer);

	public override string SystemName => AnalyzerSystemName;

	public override ILocalizableMessage NameLocalizationMessage => MarkupRangeAnalyzerNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		MarkupRangeAnalyzerDescriptionMessage.Instance;

	public override async Task<IReadOnlyList<MarkupRangeDraft>> AnalyzeAsync(
		MarkupAnalyzerInput input,
		CancellationToken cancellationToken = default)
	{
		var buckets = new List<MarkupBucketBuilder>();
		var currentBucket = new MarkupBucketBuilder();
		MarkupRangeCursor? cursor = null;

		while (true)
		{
			var result = await LoadBatch(
				input,
				cursor,
				cancellationToken);

			if (result.Count == 0)
				break;

			foreach (var row in result)
				TryAddToBuckets(
					row,
					buckets,
					ref currentBucket);

			cursor = MarkupRangeCursor.From(result[^1]);
		}

		if (currentBucket.Count > 0)
		{
			if (currentBucket.Count < MinBucketSampleSize && buckets.Count > 0)
				buckets[^1].Merge(currentBucket);
			else
				buckets.Add(currentBucket);
		}

		return BuildRanges(buckets);
	}

	private static IReadOnlyList<MarkupRangeDraft> BuildRanges(IReadOnlyList<MarkupBucketBuilder> buckets)
	{
		var ranges = new List<MarkupRangeDraft>(buckets.Count);
		for (var index = 0; index < buckets.Count; index++)
		{
			var bucket = buckets[index];
			var fromCost = index == 0
				? bucket.FromCost
				: ranges[^1].ToCost;
			var toCost = index == buckets.Count - 1
				? bucket.ToCost
				: bucket.ToCost + (buckets[index + 1].FromCost - bucket.ToCost) / 2;

			ranges.Add(bucket.Build() with { FromCost = fromCost, ToCost = toCost });
		}

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
		var query = repository.Query.AsNoTracking().ExcludeDeleted().Where(x => x.Details.Any());

		if (input.StartDate is not null)
			query = query.Where(x => x.Sale.CreatedAt >= input.StartDate);

		if (input.EndDate is not null)
			query = query.Where(x => x.Sale.CreatedAt <= input.EndDate);

		return query.Select(x => new SaleContentMarkupRow
		{
			Id = x.Id,
			SaleUnitPriceBase = Math.Round(x.PriceInBaseCurrency / (1 - x.Discount), DecimalScale),
			AvgBuyPriceBase = Math.Round(
				x.Details.Sum(d => d.BuyPriceInBaseCurrency * d.Count) / x.Details.Sum(d => d.Count),
				DecimalScale)
		});
	}

	private static void TryAddToBuckets(
		SaleContentMarkupRow row,
		List<MarkupBucketBuilder> buckets,
		ref MarkupBucketBuilder currentBucket)
	{
		if (!TryCalculateMarkup(row, out var markup))
			return;

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
				testBucket))
		{
			currentBucket = testBucket;
			return;
		}

		buckets.Add(currentBucket);

		currentBucket = new MarkupBucketBuilder();
		currentBucket.Add(row.AvgBuyPriceBase, markup);
	}

	private static bool TryCalculateMarkup(SaleContentMarkupRow row, out decimal markup)
	{
		markup = 0;

		if (row.AvgBuyPriceBase <= 0)
			return false;

		markup = (row.SaleUnitPriceBase - row.AvgBuyPriceBase) / row.AvgBuyPriceBase;
		return markup is >= MinAllowedMarkup and <= MaxAllowedMarkup;
	}

	private static bool ShouldStartNewBucket(
		SaleContentMarkupRow row,
		MarkupBucketBuilder currentBucket,
		MarkupBucketBuilder testBucket)
	{
		if (currentBucket.Count < MinBucketSampleSize || row.AvgBuyPriceBase == currentBucket.ToCost)
			return false;

		var costRatioExceeded = currentBucket.FromCost > 0 &&
			row.AvgBuyPriceBase / currentBucket.FromCost > MaxCostRatio;

		var stdDevExceeded = testBucket.StdDev > MaxStdDev;

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
		public static MarkupRangeCursor From(SaleContentMarkupRow row) => new(row.Id, row.AvgBuyPriceBase);
	}
}
