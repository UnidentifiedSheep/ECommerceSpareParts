using BulkValidation.Core.Attributes;
using Main.Enums;

namespace Main.Entities;

public partial class StorageRoute
{
    [Validate]
    public Guid Id { get; set; }

    [ValidateTuple("FromTo")]
    public string FromStorageName { get; set; } = null!;

    [ValidateTuple("FromTo")]
    public string ToStorageName { get; set; } = null!;

    public int DistanceM { get; set; }

    public RouteType RouteType { get; set; }

    public LogisticPricingType PricingModel { get; set; }

    public int DeliveryTimeMinutes { get; set; }

    public decimal PriceKg { get; set; }

    public decimal PricePerM3 { get; set; }

    public decimal PricePerOrder { get; set; }

    [ValidateTuple("FromTo")]
    public bool IsActive { get; set; }

    public int CurrencyId { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual Storage FromStorageNameNavigation { get; set; } = null!;

    public virtual ICollection<PurchaseLogistic> PurchaseLogistics { get; set; } = new List<PurchaseLogistic>();

    public virtual Storage ToStorageNameNavigation { get; set; } = null!;
}
