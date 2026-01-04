namespace Main.Core.Entities;

public partial class StorageRoute
{
    public Guid Id { get; set; }

    public string FromStorageName { get; set; } = null!;

    public string ToStorageName { get; set; } = null!;

    public int DistanceM { get; set; }

    public string RouteType { get; set; } = null!;

    public string PricingModel { get; set; } = null!;

    public int DeliveryTimeMinutes { get; set; }

    public decimal PriceKg { get; set; }

    public decimal PricePerM3 { get; set; }

    public decimal PricePerOrder { get; set; }

    public string Status { get; set; } = null!;

    public virtual Storage FromStorageNameNavigation { get; set; } = null!;

    public virtual Storage ToStorageNameNavigation { get; set; } = null!;
}
