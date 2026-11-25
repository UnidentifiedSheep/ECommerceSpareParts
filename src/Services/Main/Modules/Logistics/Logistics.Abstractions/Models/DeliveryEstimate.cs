namespace Main.Modules.Logistics.Abstractions.Models;

public readonly record struct DeliveryEstimate(DateOnly ArrivalDate, int DeliveryDays);
