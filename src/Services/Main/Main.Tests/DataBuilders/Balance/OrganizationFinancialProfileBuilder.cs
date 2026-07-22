using Bogus;
using Main.Entities.Balance;
using Main.Entities.Organization;
using Tests.Abstractions;

namespace Tests.DataBuilders.Balance;

public class OrganizationFinancialProfileBuilder(Faker faker)
    : BuilderBase<OrganizationFinancialProfile>(faker)
{
    public Guid? OrganizationId { get; private set; }
    public decimal? MinAllowedBalance { get; private set; }

    public OrganizationFinancialProfileBuilder WithOrganizationId(Guid organizationId)
    {
        OrganizationId = organizationId;
        return this;
    }

    public OrganizationFinancialProfileBuilder WithMinAllowedBalance(decimal minAllowedBalance)
    {
        MinAllowedBalance = minAllowedBalance;
        return this;
    }

    public override OrganizationFinancialProfile Build()
    {
        return OrganizationFinancialProfile.Create(
            OrganizationId ?? Guid.NewGuid(),
            MinAllowedBalance ?? 0m);
    }
}
