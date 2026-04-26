using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.User;

public class UserBalance : AuditableEntity<UserBalance, UserBalanceKey>
{
    public Guid UserId { get; private set; }
    public int CurrencyId { get; private set; }
    public decimal Balance { get; private set; }
    public uint RowVersion { get; private set; }
    
    private UserBalance() {}
    
    private UserBalance(Guid userId, int currencyId)
    {
        UserId = userId;
        CurrencyId = currencyId;
        Balance = 0;
    }

    public static UserBalance Create(Guid userId, int currencyId)
    {
        return new UserBalance(userId, currencyId);
    }

    internal void IncrementBalance(decimal amount)
    {
        amount.AgainstTooManyDecimalPlaces(
                maxDecimals: 2, 
                exceptionFactory: () => new InvalidOperationException("Amount can not have more than 2 decimal places"));
        
        Balance += amount;
    }

    public override UserBalanceKey GetId() => new(UserId, CurrencyId);
}

public readonly struct UserBalanceKey(Guid userId, int currencyId) : ICompositeKey
{
    public Guid UserId => userId;
    public int CurrencyId => currencyId;
    
    public object[] ToArray() => [userId, currencyId];
}