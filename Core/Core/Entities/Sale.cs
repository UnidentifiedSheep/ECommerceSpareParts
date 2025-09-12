namespace Core.Entities;

public class Sale
{
    public string Id { get; set; } = null!;

    public string CreatedUserId { get; set; } = null!;

    public string BuyerId { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime SaleDatetime { get; set; }

    public DateTime CreationDatetime { get; set; }

    public DateTime? UpdateDatetime { get; set; }

    public int CurrencyId { get; set; }

    public string TransactionId { get; set; } = null!;

    public string? UpdatedUserId { get; set; }

    public string MainStorageName { get; set; } = null!;

    public virtual AspNetUser Buyer { get; set; } = null!;

    public virtual AspNetUser CreatedUser { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Storage MainStorageNameNavigation { get; set; } = null!;

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual AspNetUser? UpdatedUser { get; set; }
}