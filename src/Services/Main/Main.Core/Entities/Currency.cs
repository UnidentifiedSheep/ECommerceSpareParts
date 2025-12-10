namespace Main.Core.Entities;

public partial class Currency
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CurrencySign { get; set; } = null!;

    public string Code { get; set; } = null!;

    public virtual ICollection<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; } = new List<ArticleSupplierBuyInfo>();

    public virtual ICollection<CurrencyHistory> CurrencyHistories { get; set; } = new List<CurrencyHistory>();

    public virtual CurrencyToUsd? CurrencyToUsd { get; set; }

    public virtual ICollection<MarkupGroup> MarkupGroups { get; set; } = new List<MarkupGroup>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<StorageContentReservation> StorageContentReservations { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageContent> StorageContents { get; set; } = new List<StorageContent>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();

    public virtual ICollection<TransactionVersion> TransactionVersions { get; set; } = new List<TransactionVersion>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserBalance> UserBalances { get; set; } = new List<UserBalance>();
}
