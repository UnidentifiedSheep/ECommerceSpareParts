namespace Core.Entities;

public class UserDiscount
{
    public string UserId { get; set; } = null!;

    public decimal? Discount { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}