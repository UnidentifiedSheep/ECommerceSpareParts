using BulkValidation.Core.Attributes;
using Domain;
using Enums;

namespace Main.Entities;

public class Purchase : AuditableEntity<Purchase, Guid>
{
    [Validate]
    public Guid Id { get; set; }

    public Guid CreatedUserId { get; set; }

    public Guid SupplierId { get; set; }

    public string? Comment { get; set; }

    public DateTime PurchaseDatetime { get; set; }

    public int CurrencyId { get; set; }

    public Guid TransactionId { get; set; }

    public Guid? UpdatedUserId { get; set; }

    public string Storage { get; set; } = null!;

    public PurchaseState State { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();

    public virtual PurchaseLogistic? PurchaseLogistic { get; set; }

    public virtual User Supplier { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public override Guid GetId() => Id;
}