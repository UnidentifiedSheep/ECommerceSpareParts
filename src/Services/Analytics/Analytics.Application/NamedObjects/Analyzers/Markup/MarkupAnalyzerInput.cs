namespace Analytics.Application.NamedObjects.Analyzers.Markup;

public record MarkupAnalyzerInput
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}