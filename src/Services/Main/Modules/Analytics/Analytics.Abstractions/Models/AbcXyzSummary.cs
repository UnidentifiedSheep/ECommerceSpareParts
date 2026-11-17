namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record AbcXyzSummary(
	int ProductsCount,
	int PeriodsCount,
	decimal TotalValue,
	IReadOnlyList<AbcXyzCell> Cells);

public sealed record AbcXyzCell(
	AbcClass Abc,
	XyzClass? Xyz,
	int ProductsCount,
	decimal Value);
