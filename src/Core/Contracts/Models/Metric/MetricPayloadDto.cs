namespace Contracts.Models.Metric;

public record MetricPayloadDto
{
    //Fields for all metrics.
    public required int CurrencyId { get; init; }
    public required DateTime RangeStart { get; init; }
    public required DateTime RangeEnd { get; init; }
    
    //Fields based on article
    public int? ArticleId { get; init; }
}