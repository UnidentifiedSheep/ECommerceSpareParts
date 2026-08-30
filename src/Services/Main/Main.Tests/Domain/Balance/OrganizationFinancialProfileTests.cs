using Exceptions;
using FluentAssertions;
using Main.Entities.Organization;

namespace Tests.Domain.Balance;

public class OrganizationFinancialProfileTests
{
	[Fact]
	public void Create_ValidData_SetsInitialValues()
	{
		var userId = Guid.NewGuid();

		var profile = OrganizationFinancialProfile.Create(userId, -100m);

		profile.OrganizationId.Should().Be(userId);
		profile.GetId().Should().Be(userId);
		profile.MinAllowedBalance.Should().Be(-100m);
	}

	[Fact]
	public void SetMinAllowedBalance_ValidValue_UpdatesMinimum()
	{
		var profile = OrganizationFinancialProfile.Create(Guid.NewGuid());

		profile.SetMinAllowedBalance(-10.25m);

		profile.MinAllowedBalance.Should().Be(-10.25m);
	}

	[Fact]
	public void SetMinAllowedBalance_Zero_UpdatesMinimum()
	{
		var profile = OrganizationFinancialProfile.Create(Guid.NewGuid(), -10m);

		profile.SetMinAllowedBalance(0m);

		profile.MinAllowedBalance.Should().Be(0m);
	}

	[Fact]
	public void SetMinAllowedBalance_PositiveValue_Throws()
	{
		var profile = OrganizationFinancialProfile.Create(Guid.NewGuid(), -10m);

		var act = () => profile.SetMinAllowedBalance(0.01m);

		act.Should().Throw<InvalidInputException>();
		profile.MinAllowedBalance.Should().Be(-10m);
	}

	[Fact]
	public void SetMinAllowedBalance_MoreThanTwoDecimals_Throws()
	{
		var profile = OrganizationFinancialProfile.Create(Guid.NewGuid(), -10m);

		var act = () => profile.SetMinAllowedBalance(-1.123m);

		act.Should().Throw<InvalidInputException>();
		profile.MinAllowedBalance.Should().Be(-10m);
	}

	[Theory]
	[InlineData(125.25)]
	[InlineData(0)]
	[InlineData(-125.25)]
	public void SetApproximateBalance_ValidValue_UpdatesBalance(decimal balance)
	{
		var profile = OrganizationFinancialProfile.Create(Guid.NewGuid());

		profile.SetApproximateBalance(balance);

		profile.ApproximateBalance.Should().Be(balance);
	}

	[Fact]
	public void SetApproximateBalance_MoreThanTwoDecimals_ThrowsAndKeepsPreviousBalance()
	{
		var profile = OrganizationFinancialProfile.Create(Guid.NewGuid());
		profile.SetApproximateBalance(10.25m);

		var act = () => profile.SetApproximateBalance(10.251m);

		act.Should().Throw<InvalidOperationException>();
		profile.ApproximateBalance.Should().Be(10.25m);
	}

	[Fact]
	public void Create_PositiveMinAllowedBalance_Throws()
	{
		var act = () => OrganizationFinancialProfile.Create(Guid.NewGuid(), 0.01m);

		act.Should().Throw<InvalidInputException>();
	}

	[Fact]
	public void Create_MinAllowedBalanceWithMoreThanTwoDecimals_Throws()
	{
		var act = () => OrganizationFinancialProfile.Create(Guid.NewGuid(), -1.123m);

		act.Should().Throw<InvalidInputException>();
	}
}
