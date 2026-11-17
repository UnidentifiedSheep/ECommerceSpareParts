using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Interfaces;
using Main.Modules.Analytics.Application.Models;

namespace Main.Modules.Analytics.Application.Analyzers.AbcXyz;

public sealed class AbcXyzCalculator : IAbcXyzCalculator
{
	private const int Scale = 10;

	public AbcXyzAnalysisResult Calculate(
		IEnumerable<ProductPeriodValue> rows,
		AbcXyzParameters parameters)
	{
		var periods = BuildPeriods(parameters);
		var indexByKey = periods
			.Select((period, index) => (Key: MonthKey(period), Index: index))
			.ToDictionary(item => item.Key, item => item.Index);

		var series = new Dictionary<int, ProductSeries>();
		foreach (var row in rows)
		{
			if (!indexByKey.TryGetValue(MonthKey(row.PeriodStart), out var index))
				continue;
			if (!series.TryGetValue(row.ProductId, out var product))
				series[row.ProductId] = product = new ProductSeries(periods.Count);
			product.Add(index, row.Quantity, row.Revenue);
		}

		var ordered = series
			.Select(pair => (
				ProductId: pair.Key,
				Series: pair.Value,
				Value: parameters.AbcMetric == AnalysisMetric.Revenue
					? pair.Value.TotalRevenue
					: pair.Value.TotalQuantity))
			.OrderByDescending(item => item.Value)
			.ThenBy(item => item.ProductId)
			.ToList();

		var total = ordered.Where(item => item.Value > 0).Sum(item => item.Value);
		var aCutoff = parameters.AShare;
		var bCutoff = parameters.AShare + parameters.BShare;

		var items = new List<AbcXyzItem>(ordered.Count);
		var cumulative = 0m;
		for (var i = 0; i < ordered.Count; i++)
		{
			var (productId, product, value) = ordered[i];
			var positive = value > 0 && total > 0;
			var share = positive ? Math.Round(value / total, Scale) : 0m;
			var abc = !positive
				? AbcClass.C
				: cumulative < aCutoff
					? AbcClass.A
					: cumulative < bCutoff
						? AbcClass.B
						: AbcClass.C;
			cumulative += share;

			var (xyz, coefficient, count) = ClassifyXyz(product, parameters);
			items.Add(
				new AbcXyzItem(
					productId,
					i + 1,
					product.TotalQuantity,
					product.TotalRevenue,
					value,
					share,
					Math.Min(1m, cumulative),
					abc,
					xyz,
					coefficient,
					count));
		}

		return new AbcXyzAnalysisResult(items, BuildSummary(items, periods.Count, total));
	}

	private static (XyzClass? Class, decimal? Coefficient, int Periods) ClassifyXyz(
		ProductSeries product,
		AbcXyzParameters parameters)
	{
		var first = product.FirstSaleIndex;
		var count = product.Length - first;
		if (count < parameters.MinPeriods)
			return (null, null, count);

		var values = parameters.XyzMetric == AnalysisMetric.Quantity ? product.Quantity : product.Revenue;
		var mean = 0m;
		for (var i = first; i < product.Length; i++)
			mean += values[i];
		mean /= count;
		if (mean <= 0)
			return (null, null, count);

		var variance = 0m;
		for (var i = first; i < product.Length; i++)
		{
			var delta = values[i] - mean;
			variance += delta * delta;
		}

		variance /= count;
		var coefficient = Math.Round((decimal)Math.Sqrt((double)variance) / mean, Scale);
		var xyz = coefficient <= parameters.XCoefficient
			? XyzClass.X
			: coefficient <= parameters.YCoefficient
				? XyzClass.Y
				: XyzClass.Z;
		return (xyz, coefficient, count);
	}

	private static AbcXyzSummary BuildSummary(IReadOnlyList<AbcXyzItem> items, int periodsCount, decimal total)
	{
		var cells = new List<AbcXyzCell>();
		foreach (var abc in Enum.GetValues<AbcClass>())
		foreach (var xyz in Enum.GetValues<XyzClass>().Select(x => (XyzClass?)x).Append(null))
		{
			var matching = items.Where(item => item.AbcClass == abc && item.XyzClass == xyz).ToList();
			cells.Add(new AbcXyzCell(abc, xyz, matching.Count, matching.Sum(item => item.Value)));
		}

		return new AbcXyzSummary(items.Count, periodsCount, total, cells);
	}

	private static List<DateTime> BuildPeriods(AbcXyzParameters parameters)
	{
		var periods = new List<DateTime>();
		for (var period = parameters.From; period < parameters.EndExclusive(); period = period.AddMonths(1))
			if (parameters.Months is not { Count: > 0 } || parameters.Months.Contains((Month)period.Month))
				periods.Add(period);

		return periods;
	}

	private static int MonthKey(DateTime value) => value.Year * 12 + value.Month - 1;

	private sealed class ProductSeries(int length)
	{
		private readonly bool[] _hasSales = new bool[length];

		public int Length { get; } = length;

		public decimal[] Quantity { get; } = new decimal[length];

		public decimal[] Revenue { get; } = new decimal[length];

		public decimal TotalQuantity => Quantity.Sum();

		public decimal TotalRevenue => Revenue.Sum();

		public int FirstSaleIndex => Array.IndexOf(_hasSales, true);

		public void Add(int index, decimal quantity, decimal revenue)
		{
			_hasSales[index] = true;
			Quantity[index] += quantity;
			Revenue[index] += revenue;
		}
	}
}
