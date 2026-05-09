using BulkValidation.Core.Attributes;
using Domain;

namespace Analytics.Entities;

public class PurchasesFact : Entity<PurchasesFact, Guid>
{
    [Validate]
    public Guid Id { get; set; }
    public int CurrencyId { get; set; }
    public Guid SupplierId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public decimal TotalSum { get; set; }
    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();
    public override Guid GetId() => Id;
}