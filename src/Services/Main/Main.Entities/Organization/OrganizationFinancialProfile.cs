using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Organization;

public class OrganizationFinancialProfile : AuditableEntity<OrganizationFinancialProfile, Guid>, IVersionable<uint>
{
    private OrganizationFinancialProfile(Guid organizationId, decimal minAllowedBalance)
    {
        OrganizationId = organizationId;
        SetMinAllowedBalance(minAllowedBalance);
    }
    
    private OrganizationFinancialProfile() { }

    public Guid OrganizationId { get; private set; }
    public decimal MinAllowedBalance { get; private set; }
    public uint RowVersion { get; private set; }

    public override Guid GetId() { return OrganizationId; }

    public static OrganizationFinancialProfile Create(Guid organizationId, decimal minAllowedBalance = 0)
    {
        return new OrganizationFinancialProfile(organizationId, minAllowedBalance);
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
