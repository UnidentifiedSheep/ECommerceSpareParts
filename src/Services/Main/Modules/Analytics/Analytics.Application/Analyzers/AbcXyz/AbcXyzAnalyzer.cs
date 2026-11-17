using FluentValidation;
using Main.Modules.Analytics.Abstractions.Interfaces;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Interfaces;
using Main.Modules.Analytics.Application.Services;

namespace Main.Modules.Analytics.Application.Analyzers.AbcXyz;

public sealed class AbcXyzAnalyzer(
	AbcXyzDataReader reader,
	IAbcXyzCalculator calculator,
	IValidator<AbcXyzParameters> validator) : IAbcXyzAnalyzer
{
	public async Task<AbcXyzAnalysisResult> AnalyzeAsync(
		AbcXyzParameters parameters,
		CancellationToken cancellationToken = default)
	{
		await validator.ValidateAndThrowAsync(parameters, cancellationToken);

		var rows = await reader.ReadAsync(parameters, cancellationToken);
		return calculator.Calculate(rows, parameters);
	}
}
