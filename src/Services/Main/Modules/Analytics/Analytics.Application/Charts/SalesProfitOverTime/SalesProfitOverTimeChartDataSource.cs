using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Domain.Validation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Analytics;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using SchemaGeneration.Abstractions;

namespace Main.Application.Analytics.Charts.SalesProfitOverTime;

public sealed class SalesProfitOverTimeChartDataSource(
	IReadRepository<SaleContentAnalytics, int> repository,
	ISchemaGenerator schemaGenerator)
	: ChartDataSourceNamedObject<SalesProfitDataPoint, SalesProfitChartQuery>(schemaGenerator)
{
	public const string DataSourceSystemName = nameof(SalesProfitOverTimeChartDataSource);

	public override string SystemName => DataSourceSystemName;

	public override ILocalizableMessage NameLocalizationMessage
		=> ChartSalesProfitOverTimeNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> ChartSalesProfitOverTimeDescriptionMessage.Instance;

	public override async Task<ChartDataResult<SalesProfitDataPoint>> QueryAsync(
		SalesProfitChartQuery queryInput,
		CancellationToken cancellationToken)
	{
		queryInput.EnsureNotNull(ChartSalesProfitQueryRequiredMessage.Instance).Validate();

		var query = repository
			.Query
			.Where(x => x.IsCompleted)
			.Where(x => x.SaleDatetime >= queryInput.StartDate && x.SaleDatetime <= queryInput.EndDate);

		if (queryInput.OrganizationId is { } organizationId)
			query = query.Where(x => x.OrganizationId == organizationId);

		if (queryInput.BuyerId is { } buyerId)
			query = query.Where(x => x.BuyerId == buyerId);

		var cursor = queryInput.GetCursor();

		var dataPoints = await BuildAggregateQuery(query, queryInput.Granularity)
			.ApplyCursor(cursor)
			.ToListAsync(cancellationToken);

		return new ChartDataResult<SalesProfitDataPoint>(dataPoints, dataPoints.GetNextCursor(cursor));
	}

	private static IQueryable<SalesProfitDataPoint> BuildAggregateQuery(
		IQueryable<SaleContentAnalytics> query,
		TimeGranularity granularity)
		=> query.GroupBy(x => new
			{
				x.SaleId, x.SaleDatetime
			})
			.Select(sale => new SaleProfitRow
			{
				SaleDatetime = sale.Key.SaleDatetime,
				Revenue = sale.Sum(x => x.RevenueInBaseCurrency),
				Cost = sale.Sum(x => x.CostInBaseCurrency),
				ProductsCount = sale.Sum(x => x.Count)
			})
			.GroupBy(x => new
			{
				x.SaleDatetime.Year,
				Month = granularity == TimeGranularity.Year ? 1 : x.SaleDatetime.Month,
				Day = granularity == TimeGranularity.Day ? x.SaleDatetime.Day : 1
			})
			.Select(group => new SalesProfitDataPoint
			{
				PeriodStart = new DateTime(
					group.Key.Year,
					group.Key.Month,
					group.Key.Day,
					0,
					0,
					0,
					DateTimeKind.Utc),
				Revenue = Math.Round(group.Sum(x => x.Revenue), 12),
				Cost = Math.Round(group.Sum(x => x.Cost), 12),
				GrossProfit = Math.Round(group.Sum(x => x.Revenue - x.Cost), 12),
				SalesCount = group.Count(),
				ProductsCount = group.Sum(x => x.ProductsCount)
			});

	private sealed record SaleProfitRow
	{
		public DateTime SaleDatetime { get; init; }
		public decimal Revenue { get; init; }
		public decimal Cost { get; init; }
		public int ProductsCount { get; init; }
	}
}
