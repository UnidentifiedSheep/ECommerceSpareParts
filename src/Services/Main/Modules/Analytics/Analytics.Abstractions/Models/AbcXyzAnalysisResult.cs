namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record AbcXyzAnalysisResult(
	IReadOnlyList<AbcXyzItem> Items,
	AbcXyzSummary Summary);
