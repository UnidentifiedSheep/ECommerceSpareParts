namespace Main.Core.Entities;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int CurrencyId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool BuyerApproved { get; set; }

    public bool SellerApproved { get; set; }

    public string SignedTotalPrice { get; set; } = null!;

    public Guid? WhoUpdated { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderVersion> OrderVersions { get; set; } = new List<OrderVersion>();

    public virtual User User { get; set; } = null!;

    public virtual User? WhoUpdatedNavigation { get; set; }
}
