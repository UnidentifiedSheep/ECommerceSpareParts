using FluentValidation;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Services;

namespace Main.Modules.Analytics.Application.Analyzers.AbcXyz;

public sealed class AbcXyzAnalyzer(
	AbcXyzDataReader reader,
	AbcXyzCalculator calculator,
	IValidator<AbcXyzParameters> validator)
{
	public const string AnalysisType = "AbcXyz";

	public async Task<AbcXyzAnalysisResult> AnalyzeAsync(
		AbcXyzParameters parameters,
		CancellationToken cancellationToken = default)
	{
		await validator.ValidateAndThrowAsync(parameters, cancellationToken);

		var rows = await reader.ReadAsync(parameters, cancellationToken);
		return calculator.Calculate(rows, parameters);
	}
}
