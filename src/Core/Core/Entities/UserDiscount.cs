namespace Core.Entities;

public class UserDiscount
{
    public Guid UserId { get; set; }

    public decimal? Discount { get; set; }

    public virtual User User { get; set; } = null!;
}