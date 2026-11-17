namespace Main.Modules.Analytics.Application.Models;

public sealed record AbcXyzItemData(
	decimal Quantity,
	decimal Revenue,
	decimal CumulativeShare,
	decimal? CoefficientOfVariation,
	int PeriodsCount);
