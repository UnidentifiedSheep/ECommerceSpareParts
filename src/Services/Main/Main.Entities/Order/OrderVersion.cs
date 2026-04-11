namespace Main.Entities;

public class OrderVersion
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
}