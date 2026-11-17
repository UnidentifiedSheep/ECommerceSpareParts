using System.Text.Json.Serialization;

namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record AbcXyzSummary(
	int ProductsCount,
	int PeriodsCount,
	decimal TotalValue,
	IReadOnlyList<AbcXyzCell> Cells);

public sealed record AbcXyzCell(
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AbcClass Abc,
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	XyzClass? Xyz,
	int ProductsCount,
	decimal Value);
