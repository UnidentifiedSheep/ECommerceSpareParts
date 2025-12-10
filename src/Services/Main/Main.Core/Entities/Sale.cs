namespace Main.Core.Entities;

public partial class Sale
{
    public string Id { get; set; } = null!;

    public Guid CreatedUserId { get; set; }

    public Guid BuyerId { get; set; }

    public string? Comment { get; set; }

    public DateTime SaleDatetime { get; set; }

    public DateTime CreationDatetime { get; set; }

    public DateTime? UpdateDatetime { get; set; }

    public int CurrencyId { get; set; }

    public Guid TransactionId { get; set; }

    public Guid? UpdatedUserId { get; set; }

    public string MainStorageName { get; set; } = null!;

    public string State { get; set; } = null!;

    public virtual User Buyer { get; set; } = null!;

    public virtual User CreatedUser { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Storage MainStorageNameNavigation { get; set; } = null!;

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual User? UpdatedUser { get; set; }
}
