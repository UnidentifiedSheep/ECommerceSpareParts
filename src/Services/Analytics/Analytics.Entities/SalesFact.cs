using Domain;

namespace Analytics.Entities;

public class SalesFact : AuditableEntity<SalesFact, Guid>
{
    public Guid Id { get; set; }
    public int CurrencyId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal TotalSum { get; set; }

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();
    public override Guid GetId() => Id;
}