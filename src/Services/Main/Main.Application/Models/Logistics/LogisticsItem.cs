using Enums;

namespace Main.Application.Models.Logistics;

public record LogisticsItem(
    int Id,
    int Quantity,
    decimal Weight,
    WeightUnit WeightUnit,
    decimal AreaM3
);