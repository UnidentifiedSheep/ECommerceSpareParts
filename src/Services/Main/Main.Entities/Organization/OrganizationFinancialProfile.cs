using Domain;
using Domain.Interfaces;
using Domain.Validation;

namespace Main.Entities.Organization;

public class OrganizationFinancialProfile : AuditableEntity<OrganizationFinancialProfile, Guid>,
	IVersionable<uint>
{
	private OrganizationFinancialProfile(Guid organizationId, decimal minAllowedBalance)
	{
		OrganizationId = organizationId;
		SetMinAllowedBalance(minAllowedBalance);
	}

	private OrganizationFinancialProfile()
	{
	}

	public Guid OrganizationId { get; }

	public decimal MinAllowedBalance { get; private set; }

	public decimal ApproximateBalance { get; private set; }

	public uint RowVersion { get; private set; }

	public override Guid GetId() => OrganizationId;

	public static OrganizationFinancialProfile Create(Guid organizationId, decimal minAllowedBalance = 0) =>
		new(organizationId, minAllowedBalance);

	public void SetMinAllowedBalance(decimal minAllowedBalance)
	{
		minAllowedBalance
			.EnsureMaxDecimalPlaces(2, "financial.profile.min.allowed.balance.max.two.decimal.places")
			.EnsureNonPositive("financial.profile.min.allowed.balance.must.not.be.positive");
		MinAllowedBalance = minAllowedBalance;
	}

	public void SetApproximateBalance(decimal approximateBalance)
	{
		ApproximateBalance = approximateBalance.EnsureMaxDecimalPlaces(
			2,
			() => new InvalidOperationException("ApproximateBalance must have maximum 2 decimal places"));
	}
}
