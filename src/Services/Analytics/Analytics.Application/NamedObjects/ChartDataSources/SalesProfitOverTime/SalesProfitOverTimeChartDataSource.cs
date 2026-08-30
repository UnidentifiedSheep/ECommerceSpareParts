using Analytics.Application.Extensions;
using Analytics.Application.Models;
using Analytics.Entities;
using Analytics.Entities.Enums;
using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Domain.Validation;
using Microsoft.EntityFrameworkCore;
using SchemaGeneration.Abstractions;

namespace Analytics.Application.NamedObjects.ChartDataSources.SalesProfitOverTime;

public sealed class SalesProfitOverTimeChartDataSource(
    IReadRepository<SalesFact, Guid> repository,
    ISchemaGenerator schemaGenerator) :
    ChartDataSourceNamedObject<SalesProfitDataPoint, SalesProfitChartQuery>(schemaGenerator)
{
    public const string DataSourceSystemName = nameof(SalesProfitOverTimeChartDataSource);

    public override string NameLocalizationKey => "chart.sales.profit.over.time.name";
    public override string DescriptionLocalizationKey => "chart.sales.profit.over.time.description";
    public override string SystemName => DataSourceSystemName;

    public override async Task<ChartDataResult<SalesProfitDataPoint>> QueryAsync(
        SalesProfitChartQuery queryInput,
        CancellationToken cancellationToken)
    {
        queryInput
            .EnsureNotNull("chart.sales.profit.query.required")
            .Validate();

        var query = repository.Query
            .ExcludeDeleted()
            .Where(x => x.CreatedAt >= queryInput.StartDate && x.CreatedAt <= queryInput.EndDate);

        if (queryInput.OrganizationId is { } organizationId)
            query = query.Where(x => x.OrganizationId == organizationId);

        if (queryInput.BuyerId is { } buyerId)
            query = query.Where(x => x.BuyerId == buyerId);

        var cursor = queryInput.GetCursor();
        
        var dataPoints = await BuildAggregateQuery(query, queryInput.Granularity)
            .ApplyCursor(cursor)
            .ToListAsync(cancellationToken);

        return new ChartDataResult<SalesProfitDataPoint>(
            dataPoints,
            dataPoints.GetNextCursor(cursor));
    }

    private static IQueryable<SalesProfitDataPoint> BuildAggregateQuery(
        IQueryable<SalesFact> query,
        TimeGranularity granularity)
    {
        return query
            .GroupBy(x => new
            {
                x.CreatedAt.Year,
                Month = granularity == TimeGranularity.Year
                    ? 1
                    : x.CreatedAt.Month,
                Day = granularity == TimeGranularity.Day
                    ? x.CreatedAt.Day
                    : 1
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
                Revenue = Math.Round(
                    group.Sum(x => x.RevenueInBaseCurrency),
                    12),
                Cost = Math.Round(
                    group.Sum(x => x.CostInBaseCurrency),
                    12),
                GrossProfit = Math.Round(
                    group.Sum(x => x.GrossProfitInBaseCurrency),
                    12),
                SalesCount = group.Count(),
                ProductsCount = group.Sum(x => x.ProductsCount)
            });
    }

}
