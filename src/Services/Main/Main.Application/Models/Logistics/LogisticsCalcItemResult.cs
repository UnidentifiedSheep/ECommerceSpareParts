using Enums.Units;

namespace Main.Application.Models.Logistics;

public record LogisticsCalcItemResult(
	int Id,
	decimal Cost,
	int Quantity,
	decimal AreaM3,
	decimal AreaPerItem,
	decimal Weight,
	decimal WeightPerItem,
	WeightUnit WeightUnit,
	bool Skipped,
	IReadOnlyCollection<string>? Reasons);
