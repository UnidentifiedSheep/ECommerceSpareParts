namespace Main.Core.Entities;

public partial class UserBalance
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Balance { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
