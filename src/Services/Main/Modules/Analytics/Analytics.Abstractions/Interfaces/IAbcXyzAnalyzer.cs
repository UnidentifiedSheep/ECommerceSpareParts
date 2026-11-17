using Main.Modules.Analytics.Abstractions.Models;

namespace Main.Modules.Analytics.Abstractions.Interfaces;

public interface IAbcXyzAnalyzer
{
	Task<AbcXyzAnalysisResult> AnalyzeAsync(
		AbcXyzParameters parameters,
		CancellationToken cancellationToken = default);
}
