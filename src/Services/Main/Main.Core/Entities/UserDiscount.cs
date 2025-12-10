namespace Main.Core.Entities;

public partial class UserDiscount
{
    public Guid UserId { get; set; }

    public decimal? Discount { get; set; }

    public virtual User User { get; set; } = null!;
}
