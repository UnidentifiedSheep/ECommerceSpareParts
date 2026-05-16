using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Analytics.Entities;

public class SalesFact : AuditableEntity<SalesFact, Guid>, ILinqEntity<SalesFact, Guid>
{
    public Guid Id { get; set; }
    public int CurrencyId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal TotalSum { get; set; }

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();

    public override Guid GetId()
    {
        return Id;
    }

    public static Expression<Func<SalesFact, Guid>> GetKeySelector()
        => x => x.Id;

    public static Expression<Func<SalesFact, bool>> GetEqualityExpression(Guid key)
        => x => x.Id == key;
}
