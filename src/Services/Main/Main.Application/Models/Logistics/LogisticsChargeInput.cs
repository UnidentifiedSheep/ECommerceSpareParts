namespace Main.Abstractions.Models.Logistics;

/// <param name="AreaM3">Area of ALL ITEMS (area for 1 item * quantity)</param>
/// <param name="WeightKg">Weight of ALL ITEMS (weight for 1 item * quantity)</param>
/// <param name="Quantity">Count of items</param>
public record LogisticsChargeInput(decimal AreaM3, decimal WeightKg, int Quantity);