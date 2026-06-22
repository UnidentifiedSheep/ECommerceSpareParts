using Bogus;
using Main.Entities.Balance;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Balance;

public class UserFinancialProfileBuilder(Faker faker) : BuilderBase<UserFinancialProfile>(faker)
{
    public Guid? UserId { get; private set; }
    public decimal? MinAllowedBalance { get; private set; }
    public decimal? Balance { get; private set; }

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

    public UserFinancialProfileBuilder WithBalance(decimal balance)
    {
        Balance = balance;
        return this;
    }

    public override UserFinancialProfile Build()
    {
        var profile = UserFinancialProfile.Create(
            UserId ?? Guid.NewGuid(),
            MinAllowedBalance ?? 0m);

        if (!Balance.HasValue)
            return profile;

        if (Balance.Value >= 0)
            profile.Credit(Balance.Value);
        else
            profile.Debit(Math.Abs(Balance.Value), true);

        return profile;
    }
}
