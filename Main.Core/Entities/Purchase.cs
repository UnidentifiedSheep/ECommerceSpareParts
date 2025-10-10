namespace Main.Core.Entities;

public class Purchase
{
    public string Id { get; set; } = null!;

    public Guid CreatedUserId { get; set; }

    public Guid SupplierId { get; set; }

    public string? Comment { get; set; }

    public DateTime PurchaseDatetime { get; set; }

    public DateTime CreationDatetime { get; set; }

    public DateTime? UpdateDatetime { get; set; }

    public int CurrencyId { get; set; }

    public string TransactionId { get; set; } = null!;

    public Guid? UpdatedUserId { get; set; }

    public string Storage { get; set; } = null!;

    public virtual User CreatedUser { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();

    public virtual Storage StorageNavigation { get; set; } = null!;

    public virtual User Supplier { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual User? UpdatedUser { get; set; }
}