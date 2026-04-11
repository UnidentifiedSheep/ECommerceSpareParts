namespace Main.Entities;

public class UserDiscount
{
    public Guid UserId { get; set; }

    public decimal? Discount { get; set; }
}