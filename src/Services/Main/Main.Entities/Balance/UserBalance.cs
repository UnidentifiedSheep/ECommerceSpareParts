namespace Main.Entities.User;

public class UserBalance
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int CurrencyId { get; set; }

    public decimal Balance { get; set; }
    
    public uint RowVersion { get; set; }
}