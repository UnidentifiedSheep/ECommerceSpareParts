namespace Core.Entities;

public partial class Purchase
{
    public string Id { get; set; } = null!;

    public string CreatedUserId { get; set; } = null!;

    public string SupplierId { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime PurchaseDatetime { get; set; }

    public DateTime CreationDatetime { get; set; }

    public DateTime? UpdateDatetime { get; set; }

    public int CurrencyId { get; set; }

    public string TransactionId { get; set; } = null!;

    public string? UpdatedUserId { get; set; }

    public string Storage { get; set; } = null!;

    public virtual AspNetUser CreatedUser { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();

    public virtual Storage StorageNavigation { get; set; } = null!;

    public virtual AspNetUser Supplier { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual AspNetUser? UpdatedUser { get; set; }
}
