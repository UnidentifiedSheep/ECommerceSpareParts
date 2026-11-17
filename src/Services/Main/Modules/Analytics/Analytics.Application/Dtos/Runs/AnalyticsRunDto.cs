namespace Main.Modules.Analytics.Application.Dtos.Runs;

public sealed record AnalyticsRunDto
{
	public required Guid Id { get; init; }

	public required string AnalysisType { get; init; }

	public required int SchemaVersion { get; init; }

	public required string Parameters { get; init; }

	public string? Summary { get; init; }

	public required int ItemsCount { get; init; }

	public DateTime? PeriodStart { get; init; }

	public DateTime? PeriodEnd { get; init; }

	public Guid? JobId { get; init; }

	public Guid? CreatedBy { get; init; }

	public required bool IsPinned { get; init; }

	public required DateTime CreatedAt { get; init; }
}
