using Main.Enums;

namespace Main.Abstractions.Models.Logistics;

public record LogisticsCalcItemResult(int Id, decimal Cost, int Quantity, decimal AreaM3, decimal AreaPerItem, 
    decimal Weight, decimal WeightPerItem, WeightUnit WeightUnit, bool Skipped, string? Reason);