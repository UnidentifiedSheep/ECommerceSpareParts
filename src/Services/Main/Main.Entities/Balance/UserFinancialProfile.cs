using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Balance;

public class UserFinancialProfile : AuditableEntity<UserFinancialProfile, Guid>, IVersionable<uint>
{
    public Guid UserId { get; private set; }
    public decimal WalletBalance { get; private set; }
    public decimal SystemBalance { get; private set; }
    public decimal AvailableBalance => WalletBalance + SystemBalance;
    public decimal MinAllowedBalance { get; private set; }
    public uint RowVersion { get; private set; }

    public override Guid GetId() => UserId;

    private UserFinancialProfile(Guid userId, decimal minAllowedBalance)
    {
        UserId = userId;
        WalletBalance = 0;
        SystemBalance = 0;
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

    public void DepositWallet(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");
        WalletBalance += amount;
    }

    public void SpendAvailable(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");
        var newAvailableBalance = AvailableBalance - amount;
        newAvailableBalance.AgainstTooSmall(
            MinAllowedBalance,
            "financial.profile.balance.must.not.be.less.than.minimum");

        var fromWallet = Math.Min(WalletBalance, amount);
        WalletBalance -= fromWallet;

        var rest = amount - fromWallet;
        SystemBalance -= rest;
    }

    public void IncreaseSystemBalance(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");
        SystemBalance += amount;
    }

    public void DecreaseSystemBalance(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");
        SystemBalance -= amount;
    }

    public void PayToSystem(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");

        if (SystemBalance >= 0)
        {
            SpendAvailable(amount);
            return;
        }

        var debtPayment = Math.Min(amount, -SystemBalance);
        SystemBalance += debtPayment;

        var rest = amount - debtPayment;
        if (rest > 0)
            SpendAvailable(rest);
    }

    public void ReceiveFromSystem(decimal amount)
    {
        amount.AgainstNegative("financial.profile.amount.must.not.be.negative");

        if (SystemBalance <= 0)
        {
            DepositWallet(amount);
            return;
        }

        var debtPayment = Math.Min(amount, SystemBalance);
        SystemBalance -= debtPayment;

        var rest = amount - debtPayment;
        if (rest > 0)
            DepositWallet(rest);
    }
}
