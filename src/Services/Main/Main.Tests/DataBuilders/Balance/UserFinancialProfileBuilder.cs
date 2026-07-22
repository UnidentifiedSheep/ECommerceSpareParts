using Bogus;
using Main.Entities.Balance;
using Main.Entities.Organization;
using Tests.Abstractions;

namespace Tests.DataBuilders.Balance;

public class UserFinancialProfileBuilder(Faker faker) : BuilderBase<OrganizationFinancialProfile>(faker)
{
    public Guid? UserId { get; private set; }
    public decimal? MinAllowedBalance { get; private set; }

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

    public override OrganizationFinancialProfile Build()
    {
        return OrganizationFinancialProfile.Create(
            UserId ?? Guid.NewGuid(),
            MinAllowedBalance ?? 0m);
    }
}
