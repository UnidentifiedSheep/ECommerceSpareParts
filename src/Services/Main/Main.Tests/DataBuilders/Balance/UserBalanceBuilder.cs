using Bogus;
using Main.Entities.Balance;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Balance;

public class UserBalanceBuilder(Faker faker) : BuilderBase<UserBalance>(faker)
{
    public Guid? UserId { get; private set; }
    public int? CurrencyId { get; private set; }

    public UserBalanceBuilder WithUserId(Guid userId)
    {
        UserId = userId;
        return this;
    }

    public UserBalanceBuilder WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return this;
    }

    public override UserBalance Build()
    {
        return UserBalance.Create(
            UserId ?? Guid.NewGuid(),
            CurrencyId ?? Faker.Random.Int(1, 100));
    }
}