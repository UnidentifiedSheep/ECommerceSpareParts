namespace Main.Entities.Order;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public int ProductId { get; set; }

    public int Count { get; set; }

    public string SignedPrice { get; set; } = null!;

    public decimal? LockedPrice { get; set; }
}