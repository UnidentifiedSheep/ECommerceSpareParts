using System.Linq.Expressions;
using Domain;

namespace Main.Entities.Order;

public class Order : AuditableEntity<Order, Guid>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int CurrencyId { get; set; }

    public string Status { get; set; } = null!;

    public bool BuyerApproved { get; set; }

    public bool SellerApproved { get; set; }

    public string SignedTotalPrice { get; set; } = null!;
    public bool IsCanceled { get; set; }

    public override Guid GetId()
    {
        return Id;
    }

    public override Expression<Func<Order, bool>> GetEqualityExpression(Guid key)
        =>  x => x.Id == key;
}