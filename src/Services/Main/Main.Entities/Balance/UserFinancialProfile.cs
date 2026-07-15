using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Balance;

public class UserFinancialProfile : AuditableEntity<UserFinancialProfile, Guid>, IVersionable<uint>
{
    private UserFinancialProfile(Guid userId, decimal minAllowedBalance)
    {
        UserId = userId;
        Balance = 0;
        SetMinAllowedBalance(minAllowedBalance);
    }

    public Guid UserId { get; }
    public decimal Balance { get; private set; }
    public decimal MinAllowedBalance { get; private set; }
    public uint RowVersion { get; private set; }

    public override Guid GetId() { return UserId; }

    public static UserFinancialProfile Create(Guid userId, decimal minAllowedBalance = 0)
    {
        return new UserFinancialProfile(userId, minAllowedBalance);
    }

    public void Credit(decimal amount)
    {
        amount.EnsureNonNegative("financial.profile.amount.must.not.be.negative");
        Balance += amount;
    }

    public void Debit(decimal amount, bool force = false)
    {
        amount.EnsureNonNegative("financial.profile.amount.must.not.be.negative");

        var newBalance = Balance - amount;
        if (!force)
            newBalance.EnsureAtLeast(
                MinAllowedBalance,
                "financial.profile.balance.must.not.be.less.than.minimum");

        Balance = newBalance;
    }

    public void SetMinAllowedBalance(decimal minAllowedBalance)
    {
        minAllowedBalance.EnsureMaxDecimalPlaces(
                2,
                "financial.profile.min.allowed.balance.max.two.decimal.places")
            .EnsureNonPositive("financial.profile.min.allowed.balance.must.not.be.positive");
        MinAllowedBalance = minAllowedBalance;
    }
}