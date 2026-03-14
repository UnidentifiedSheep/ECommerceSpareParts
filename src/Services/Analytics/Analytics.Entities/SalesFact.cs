namespace Analytics.Entities;

public partial class SalesFact
{
    public string Id { get; set; } = null!;

    public int CurrencyId { get; set; }

    public Guid BuyerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal TotalSum { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();
}
