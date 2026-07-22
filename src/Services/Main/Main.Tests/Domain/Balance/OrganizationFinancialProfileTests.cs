using Exceptions;
using FluentAssertions;
using Main.Entities.Balance;
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
