using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Balance;

public class OrganizationBalance : AuditableEntity<OrganizationBalance, UserBalanceKey>,
    ILinqEntity<OrganizationBalance, UserBalanceKey>
{
    private OrganizationBalance() { }

    private OrganizationBalance(Guid organizationId, int currencyId)
    {
        OrganizationId = organizationId;
        CurrencyId = currencyId;
        Balance = 0;
    }

    public Guid OrganizationId { get; }
    public int CurrencyId { get; }
    public decimal Balance { get; private set; }
    public uint RowVersion { get; private set; }
    public Currency.Currency Currency { get; private set; } = null!;

    public static Expression<Func<OrganizationBalance, UserBalanceKey>> GetKeySelector()
    {
        return x => new UserBalanceKey(x.OrganizationId, x.CurrencyId);
    }

    public static Expression<Func<OrganizationBalance, bool>> GetEqualityExpression(UserBalanceKey key)
    {
        return x => x.OrganizationId == key.OrganizationId && x.CurrencyId == key.CurrencyId;
    }

    public static OrganizationBalance Create(Guid organizationId, int currencyId)
    {
        return new OrganizationBalance(organizationId, currencyId);
    }

    internal void IncrementBalance(decimal amount)
    {
        amount.EnsureMaxDecimalPlaces(
            2,
            () => new InvalidOperationException("Amount can not have more than 2 decimal places"));

        Balance += amount;
    }

    public override UserBalanceKey GetId() { return new UserBalanceKey(OrganizationId, CurrencyId); }
}

public readonly struct UserBalanceKey(Guid organizationId, int currencyId) : ICompositeKey
{
    public Guid OrganizationId => organizationId;
    public int CurrencyId => currencyId;

    public object[] ToArray() { return [organizationId, currencyId]; }
}