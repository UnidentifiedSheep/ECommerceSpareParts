namespace Main.Entities.User;

public class UserDiscount
{
    public Guid UserId { get; set; }

    public decimal? Discount { get; set; }
}