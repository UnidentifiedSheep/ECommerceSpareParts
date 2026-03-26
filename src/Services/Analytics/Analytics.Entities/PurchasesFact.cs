using BulkValidation.Core.Attributes;

namespace Analytics.Entities;

public class PurchasesFact
{
    [Validate]
    public string Id { get; set; } = null!;

    public int CurrencyId { get; set; }

    public Guid SupplierId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ProcessedAt { get; set; }

    public decimal TotalSum { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();
}