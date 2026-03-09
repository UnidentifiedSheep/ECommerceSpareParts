namespace Analytics.Entities;

public partial class PurchasesFact
{
    public string Id { get; set; } = null!;

    public int CurrencyId { get; set; }

    public Guid SupplierId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();
}
