namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record AbcXyzItem(
	int ProductId,
	int Rank,
	decimal Quantity,
	decimal Revenue,
	decimal Value,
	decimal Share,
	decimal CumulativeShare,
	AbcClass AbcClass,
	XyzClass? XyzClass,
	decimal? CoefficientOfVariation,
	int PeriodsCount);
