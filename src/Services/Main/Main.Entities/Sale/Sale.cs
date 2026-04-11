using Domain;
using Main.Enums;

namespace Main.Entities;

public class Sale : AuditableEntity<Sale, Guid>
{
    public Guid Id { get; set; }

    public Guid CreatedUserId { get; set; }

    public Guid BuyerId { get; set; }

    public string? Comment { get; set; }

    public DateTime SaleDatetime { get; set; }

    public int CurrencyId { get; set; }

    public Guid TransactionId { get; set; }

    public Guid? UpdatedUserId { get; set; }

    public string MainStorageName { get; set; } = null!;

    public SaleState State { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();

    public virtual Transaction Transaction { get; set; } = null!;

    public override Guid GetId() => Id;
}