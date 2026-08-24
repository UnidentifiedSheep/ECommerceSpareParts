using System.Text.Json.Serialization;
using Analytics.Application.Interfaces.ChartData;
using SchemaGeneration.Abstractions.Attributes;

namespace Analytics.Application.NamedObjects.ChartDataSlices.SalesProfitOverTime;

public sealed record SalesProfitDataPoint : IChartDataPoint
{
    [JsonPropertyName("periodStart")]
    [RequiredSchemaField]
    [SchemaFieldLabel("chart.sales.profit.point.period.start.name")]
    [SchemaFieldDescription("chart.sales.profit.point.period.start.description")]
    public required DateTime PeriodStart { get; init; }

    [JsonPropertyName("revenue")]
    [RequiredSchemaField]
    [SchemaFieldLabel("chart.sales.profit.point.revenue.name")]
    [SchemaFieldDescription("chart.sales.profit.point.revenue.description")]
    public required decimal Revenue { get; init; }

    [JsonPropertyName("cost")]
    [RequiredSchemaField]
    [SchemaFieldLabel("chart.sales.profit.point.cost.name")]
    [SchemaFieldDescription("chart.sales.profit.point.cost.description")]
    public required decimal Cost { get; init; }

    [JsonPropertyName("grossProfit")]
    [RequiredSchemaField]
    [SchemaFieldLabel("chart.sales.profit.point.gross.profit.name")]
    [SchemaFieldDescription("chart.sales.profit.point.gross.profit.description")]
    public required decimal GrossProfit { get; init; }

    [JsonPropertyName("salesCount")]
    [RequiredSchemaField]
    [SchemaFieldLabel("chart.sales.profit.point.sales.count.name")]
    [SchemaFieldDescription("chart.sales.profit.point.sales.count.description")]
    public required int SalesCount { get; init; }

    [JsonPropertyName("productsCount")]
    [RequiredSchemaField]
    [SchemaFieldLabel("chart.sales.profit.point.products.count.name")]
    [SchemaFieldDescription("chart.sales.profit.point.products.count.description")]
    public required int ProductsCount { get; init; }

    [JsonPropertyName("margin")]
    [RequiredSchemaField]
    [SchemaFieldLabel("chart.sales.profit.point.margin.name")]
    [SchemaFieldDescription("chart.sales.profit.point.margin.description")]
    public decimal Margin => Revenue == 0m ? 0m : GrossProfit / Revenue;
}
