using Bogus;
using Main.Entities.Balance;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Balance;

public class UserFinancialProfileBuilder(Faker faker) : BuilderBase<UserFinancialProfile>(faker)
{
    public Guid? UserId { get; private set; }
    public decimal? MinAllowedBalance { get; private set; }
    public decimal? WalletBalance { get; private set; }
    public decimal? SystemBalance { get; private set; }

    public UserFinancialProfileBuilder WithUserId(Guid userId)
    {
        UserId = userId;
        return this;
    }

    public UserFinancialProfileBuilder WithMinAllowedBalance(decimal minAllowedBalance)
    {
        MinAllowedBalance = minAllowedBalance;
        return this;
    }

    public UserFinancialProfileBuilder WithWalletBalance(decimal walletBalance)
    {
        WalletBalance = walletBalance;
        return this;
    }

    public UserFinancialProfileBuilder WithSystemBalance(decimal systemBalance)
    {
        SystemBalance = systemBalance;
        return this;
    }

    public override UserFinancialProfile Build()
    {
        var profile = UserFinancialProfile.Create(
            UserId ?? Guid.NewGuid(),
            MinAllowedBalance ?? 0m);

        if (WalletBalance.HasValue)
            profile.DepositWallet(WalletBalance.Value);

        if (!SystemBalance.HasValue)
            return profile;

        if (SystemBalance.Value >= 0)
            profile.IncreaseSystemBalance(SystemBalance.Value);
        else
            profile.DecreaseSystemBalance(Math.Abs(SystemBalance.Value));

        return profile;
    }
}
