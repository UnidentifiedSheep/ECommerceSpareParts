namespace Main.Core.Entities;

public partial class OrderVersion
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid? WhoUpdated { get; set; }

    public int CurrencyId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public bool BuyerApproved { get; set; }

    public bool SellerApproved { get; set; }

    public string SignedTotalPrice { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual User? WhoUpdatedNavigation { get; set; }
}
