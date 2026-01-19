using BulkValidation.Core.Attributes;

namespace Main.Entities;

public partial class Currency
{
    [Validate]
    public int Id { get; set; }

    [Validate]
    public string ShortName { get; set; } = null!;

    [Validate]
    public string Name { get; set; } = null!;

    [Validate]
    public string CurrencySign { get; set; } = null!;

    [Validate]
    public string Code { get; set; } = null!;

    public virtual ICollection<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; } = new List<ArticleSupplierBuyInfo>();

    public virtual ICollection<CurrencyHistory> CurrencyHistories { get; set; } = new List<CurrencyHistory>();

    public virtual CurrencyToUsd? CurrencyToUsd { get; set; }

    public virtual ICollection<MarkupGroup> MarkupGroups { get; set; } = new List<MarkupGroup>();

    public virtual ICollection<OrderVersion> OrderVersions { get; set; } = new List<OrderVersion>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<StorageContentReservation> StorageContentReservations { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageContent> StorageContents { get; set; } = new List<StorageContent>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();

    public virtual ICollection<StorageRoute> StorageRoutes { get; set; } = new List<StorageRoute>();

    public virtual ICollection<TransactionVersion> TransactionVersions { get; set; } = new List<TransactionVersion>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserBalance> UserBalances { get; set; } = new List<UserBalance>();
    
    public virtual ICollection<PurchaseLogistic> PurchaseLogistics { get; set; } = new List<PurchaseLogistic>();
}
