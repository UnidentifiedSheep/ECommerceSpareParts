using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.StorageRoutes;

public class StorageRouteDto
{
    public Guid Id { get; set; }
    public string FromStorageName { get; set; } = null!;
    public string ToStorageName { get; set; } = null!;
    public int DistanceM { get; set; }
    public RouteType RouteType { get; set; }
    public LogisticPricingType PricingModel { get; set; }
    public int DeliveryTimeMinutes { get; set; }
    public decimal PriceKg { get; set; }
    public decimal PricePerM3 { get; set; }
    public decimal PricePerOrder { get; set; }
    public bool IsActive { get; set; }
    public int CurrencyId { get; set; }
}