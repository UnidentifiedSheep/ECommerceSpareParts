namespace Main.Entities;

public partial class PurchaseLogistic
{
    public string PurchaseId { get; set; } = null!;

    public Guid RouteId { get; set; }

    public int CurrencyId { get; set; }

    public Guid TransactionId { get; set; }

    public string PricingModel { get; set; } = null!;

    public string RouteType { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Purchase Purchase { get; set; } = null!;

    public virtual StorageRoute Route { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
