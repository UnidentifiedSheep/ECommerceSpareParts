using Analytics.Entities;
using Analytics.Entities.Enums;
using Application.Common.Interfaces.Repositories;
using Domain.Validation;
using Microsoft.EntityFrameworkCore;
using SchemaGeneration.Abstractions;

namespace Analytics.Application.NamedObjects.ChartDataSlices.SalesProfitOverTime;

public sealed class SalesProfitOverTimeChartDataSource(
    IReadRepository<SalesFact, Guid> repository,
    ISchemaGenerator schemaGenerator) :
    ChartDataSourceNamedObject<SalesProfitDataPoint, SalesProfitChartQuery>(schemaGenerator)
{
    public const string DataSourceSystemName = nameof(SalesProfitOverTimeChartDataSource);

    public override string NameLocalizationKey => "chart.sales.profit.over.time.name";
    public override string DescriptionLocalizationKey => "chart.sales.profit.over.time.description";
    public override string SystemName => DataSourceSystemName;

    public override async Task<IReadOnlyList<SalesProfitDataPoint>> QueryAsync(
        SalesProfitChartQuery queryInput,
        CancellationToken cancellationToken)
    {
        queryInput
            .EnsureNotNull("chart.sales.profit.query.required")
            .Validate();

        var query = repository.Query
            .Where(x => x.CreatedAt >= queryInput.StartDate && x.CreatedAt <= queryInput.EndDate);

        if (queryInput.OrganizationId is { } organizationId)
            query = query.Where(x => x.OrganizationId == organizationId);

        if (queryInput.BuyerId is { } buyerId)
            query = query.Where(x => x.BuyerId == buyerId);

        return await BuildAggregateQuery(query, queryInput.Granularity)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.Day)
            .Select(x => new SalesProfitDataPoint
            {
                PeriodStart = new DateTime(
                    x.Year,
                    x.Month,
                    x.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                Revenue = x.Revenue,
                Cost = x.Cost,
                GrossProfit = x.GrossProfit,
                SalesCount = x.SalesCount,
                ProductsCount = x.ProductsCount
            })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<SalesProfitAggregate> BuildAggregateQuery(
        IQueryable<SalesFact> query,
        TimeGranularity granularity)
    {
        return granularity switch
        {
            TimeGranularity.Day => query
                .GroupBy(x => new
                {
                    x.CreatedAt.Year,
                    x.CreatedAt.Month,
                    x.CreatedAt.Day
                })
                .Select(group => new SalesProfitAggregate
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Day = group.Key.Day,
                    Revenue = group.Sum(x => x.RevenueInBaseCurrency),
                    Cost = group.Sum(x => x.CostInBaseCurrency),
                    GrossProfit = group.Sum(x => x.GrossProfitInBaseCurrency),
                    SalesCount = group.Count(),
                    ProductsCount = group.Sum(x => x.ProductsCount)
                }),
            TimeGranularity.Month => query
                .GroupBy(x => new
                {
                    x.CreatedAt.Year,
                    x.CreatedAt.Month
                })
                .Select(group => new SalesProfitAggregate
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Day = 1,
                    Revenue = group.Sum(x => x.RevenueInBaseCurrency),
                    Cost = group.Sum(x => x.CostInBaseCurrency),
                    GrossProfit = group.Sum(x => x.GrossProfitInBaseCurrency),
                    SalesCount = group.Count(),
                    ProductsCount = group.Sum(x => x.ProductsCount)
                }),
            TimeGranularity.Year => query
                .GroupBy(x => x.CreatedAt.Year)
                .Select(group => new SalesProfitAggregate
                {
                    Year = group.Key,
                    Month = 1,
                    Day = 1,
                    Revenue = group.Sum(x => x.RevenueInBaseCurrency),
                    Cost = group.Sum(x => x.CostInBaseCurrency),
                    GrossProfit = group.Sum(x => x.GrossProfitInBaseCurrency),
                    SalesCount = group.Count(),
                    ProductsCount = group.Sum(x => x.ProductsCount)
                }),
            _ => throw new ArgumentOutOfRangeException(
                nameof(granularity),
                granularity,
                "Unsupported time granularity.")
        };
    }

    private sealed class SalesProfitAggregate
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public int Day { get; init; }
        public decimal Revenue { get; init; }
        public decimal Cost { get; init; }
        public decimal GrossProfit { get; init; }
        public int SalesCount { get; init; }
        public int ProductsCount { get; init; }
    }
}
