using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Order;

public class Order : AuditableEntity<Order, Guid>, ILinqEntity<Order, Guid>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int CurrencyId { get; set; }

    public string Status { get; set; } = null!;

    public bool BuyerApproved { get; set; }

    public bool SellerApproved { get; set; }

    public string SignedTotalPrice { get; set; } = null!;
    public bool IsCanceled { get; set; }

    public static Expression<Func<Order, Guid>> GetKeySelector()
    {
        return x => x.Id;
    }

    public static Expression<Func<Order, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public override Guid GetId()
    {
        return Id;
    }
}
