using Enums;
using Main.Enums;

namespace Main.Abstractions.Models.Logistics;

public record LogisticsItem(int Id, int Quantity, decimal Weight, WeightUnit WeightUnit, decimal AreaM3);