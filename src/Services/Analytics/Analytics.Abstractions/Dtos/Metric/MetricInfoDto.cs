namespace Analytics.Abstractions.Dtos.Metric;

public record MetricInfoDto
{
    public required string SystemName { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
}