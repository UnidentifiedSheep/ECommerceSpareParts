namespace Search.Abstractions.Options;

public sealed record DeletedProductsCleanupOptions
{
	public const string SectionName = "DeletedProductsCleanup";

	public int RetentionDays { get; init; } = 7;

	public int BatchSize { get; init; } = 1000;
}
