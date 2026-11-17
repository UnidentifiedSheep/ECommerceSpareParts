namespace Main.Modules.Analytics.Abstractions.Analyzers.Markup;

public record MarkupAnalyzerInput
{
	public DateTime? StartDate { get; init; }

	public DateTime? EndDate { get; init; }
}
