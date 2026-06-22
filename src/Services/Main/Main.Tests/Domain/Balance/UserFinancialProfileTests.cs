using Exceptions;
using FluentAssertions;
using Main.Entities.Balance;

namespace Tests.Domain.Balance;

public class UserFinancialProfileTests
{
    [Fact]
    public void Create_ValidData_SetsInitialValues()
    {
        var userId = Guid.NewGuid();

        var profile = UserFinancialProfile.Create(userId, 100m);

        profile.UserId.Should().Be(userId);
        profile.GetId().Should().Be(userId);
        profile.TotalBalance.Should().Be(0);
        profile.MinAllowedBalance.Should().Be(100m);
    }

    [Fact]
    public void Create_WithoutMinAllowedBalance_SetsZeroMinimum()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        profile.TotalBalance.Should().Be(0);
        profile.MinAllowedBalance.Should().Be(0);
    }

    [Fact]
    public void Deposit_PositiveAmount_IncreasesTotalBalance()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        profile.Deposit(150.25m);

        profile.TotalBalance.Should().Be(150.25m);
    }

    [Fact]
    public void Withdraw_WhenBalanceRemainsAboveMinimum_DecreasesTotalBalance()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid(), 50m);
        profile.Deposit(150m);

        profile.Withdraw(100m);

        profile.TotalBalance.Should().Be(50m);
    }

    [Fact]
    public void Withdraw_WhenBalanceWouldBecomeLessThanMinimum_Throws()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid(), 50m);
        profile.Deposit(100m);

        var act = () => profile.Withdraw(51m);

        act.Should().Throw<InvalidInputException>();
        profile.TotalBalance.Should().Be(100m);
    }

    [Fact]
    public void Withdraw_NegativeAmount_Throws()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        var act = () => profile.Withdraw(-1m);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Deposit_NegativeAmount_Throws()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        var act = () => profile.Deposit(-1m);

        act.Should().Throw<InvalidInputException>();
    }
}
