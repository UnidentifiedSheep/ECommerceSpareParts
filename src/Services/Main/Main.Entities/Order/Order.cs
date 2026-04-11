using Domain;

namespace Main.Entities;

public class Order : AuditableEntity<Order, Guid>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int CurrencyId { get; set; }

    public string Status { get; set; } = null!;

    public bool BuyerApproved { get; set; }

    public bool SellerApproved { get; set; }

    public string SignedTotalPrice { get; set; } = null!;

    public Guid? WhoUpdated { get; set; }

    public bool IsCanceled { get; set; }

    public override Guid GetId() => Id;
}