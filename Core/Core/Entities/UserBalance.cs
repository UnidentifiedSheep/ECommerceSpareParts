namespace Core.Entities;

public class UserBalance
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int CurrencyId { get; set; }

    public decimal Balance { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}