using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Balance;

public class UserFinancialProfile : AuditableEntity<UserFinancialProfile, Guid>, IVersionable<uint>
{
    public Guid UserId { get; private set; }
    public decimal TotalBalance { get; private set; }
    public decimal MinAllowedBalance { get; private set; }
    public uint RowVersion { get; private set; }

    public override Guid GetId() => UserId;

    private UserFinancialProfile(Guid userId, decimal minAllowedBalance)
    {
        UserId = userId;
        TotalBalance = 0;
        SetMinAllowedBalance(minAllowedBalance);
    }

    public static UserFinancialProfile Create(Guid userId, decimal minAllowedBalance = 0)
        => new(userId, minAllowedBalance);

    public void SetMinAllowedBalance(decimal minAllowedBalance)
    {
        minAllowedBalance.AgainstTooManyDecimalPlaces(2, "financial.profile.min.allowed.balance.max.two.decimal.places")
            .AgainstPositive("financial.profile.min.allowed.balance.must.not.be.positive");
        MinAllowedBalance = minAllowedBalance;
    }

    public void Withdraw(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");
        var newBalance = TotalBalance - amount;
        newBalance.AgainstTooSmall(MinAllowedBalance, "financial.profile.balance.must.not.be.less.than.minimum");
        TotalBalance = newBalance;
    }

    public void Deposit(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");
        var newBalance = TotalBalance + amount;
        TotalBalance = newBalance;
    }
}
