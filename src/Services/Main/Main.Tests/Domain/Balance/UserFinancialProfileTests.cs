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

        var profile = UserFinancialProfile.Create(userId, -100m);

        profile.UserId.Should().Be(userId);
        profile.GetId().Should().Be(userId);
        profile.WalletBalance.Should().Be(0);
        profile.SystemBalance.Should().Be(0);
        profile.AvailableBalance.Should().Be(0);
        profile.MinAllowedBalance.Should().Be(-100m);
    }

    [Fact]
    public void DepositWallet_PositiveAmount_IncreasesWalletAndAvailableBalance()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        profile.DepositWallet(150.25m);

        profile.WalletBalance.Should().Be(150.25m);
        profile.SystemBalance.Should().Be(0);
        profile.AvailableBalance.Should().Be(150.25m);
    }

    [Fact]
    public void IncreaseSystemBalance_WhenSystemOwesUser_IncreasesAvailableBalance()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        profile.IncreaseSystemBalance(400m);

        profile.WalletBalance.Should().Be(0);
        profile.SystemBalance.Should().Be(400m);
        profile.AvailableBalance.Should().Be(400m);
    }

    [Fact]
    public void DecreaseSystemBalance_WhenUserOwesSystem_DecreasesAvailableBalance()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid(), -100m);

        profile.DecreaseSystemBalance(75m);

        profile.WalletBalance.Should().Be(0);
        profile.SystemBalance.Should().Be(-75m);
        profile.AvailableBalance.Should().Be(-75m);
    }

    [Fact]
    public void SpendAvailable_UsesWalletBeforePositiveSystemBalance()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());
        profile.DepositWallet(300m);
        profile.IncreaseSystemBalance(400m);

        profile.SpendAvailable(700m);

        profile.WalletBalance.Should().Be(0);
        profile.SystemBalance.Should().Be(0);
        profile.AvailableBalance.Should().Be(0);
    }

    [Fact]
    public void SpendAvailable_WhenPartOfSystemBalanceRemains_DecreasesOnlyNeededAmount()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());
        profile.DepositWallet(300m);
        profile.IncreaseSystemBalance(400m);

        profile.SpendAvailable(500m);

        profile.WalletBalance.Should().Be(0);
        profile.SystemBalance.Should().Be(200m);
        profile.AvailableBalance.Should().Be(200m);
    }

    [Fact]
    public void SpendAvailable_WhenBalanceWouldBecomeLessThanMinimum_Throws()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());
        profile.DepositWallet(100m);

        var act = () => profile.SpendAvailable(101m);

        act.Should().Throw<InvalidInputException>();
        profile.WalletBalance.Should().Be(100m);
        profile.SystemBalance.Should().Be(0);
    }

    [Fact]
    public void PayToSystem_WhenUserHasDebtToSystem_ReducesSystemDebtFirst()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());
        profile.DepositWallet(300m);
        profile.DecreaseSystemBalance(200m);

        profile.PayToSystem(150m);

        profile.WalletBalance.Should().Be(300m);
        profile.SystemBalance.Should().Be(-50m);
        profile.AvailableBalance.Should().Be(250m);
    }

    [Fact]
    public void PayToSystem_WhenPaymentExceedsDebt_SpendsRestFromAvailable()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());
        profile.DepositWallet(300m);
        profile.DecreaseSystemBalance(200m);

        profile.PayToSystem(250m);

        profile.WalletBalance.Should().Be(250m);
        profile.SystemBalance.Should().Be(0m);
        profile.AvailableBalance.Should().Be(250m);
    }

    [Fact]
    public void ReceiveFromSystem_WhenSystemOwesUser_ReducesSystemDebtFirst()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());
        profile.IncreaseSystemBalance(200m);

        profile.ReceiveFromSystem(150m);

        profile.WalletBalance.Should().Be(0m);
        profile.SystemBalance.Should().Be(50m);
        profile.AvailableBalance.Should().Be(50m);
    }

    [Fact]
    public void ReceiveFromSystem_WhenPaymentExceedsSystemDebt_DepositsRestToWallet()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());
        profile.IncreaseSystemBalance(200m);

        profile.ReceiveFromSystem(250m);

        profile.WalletBalance.Should().Be(50m);
        profile.SystemBalance.Should().Be(0m);
        profile.AvailableBalance.Should().Be(50m);
    }

    [Theory]
    [InlineData(-1)]
    public void PositiveOperations_NegativeAmount_Throws(decimal amount)
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        var depositWallet = () => profile.DepositWallet(amount);
        var spendAvailable = () => profile.SpendAvailable(amount);
        var increaseSystemBalance = () => profile.IncreaseSystemBalance(amount);
        var decreaseSystemBalance = () => profile.DecreaseSystemBalance(amount);
        var payToSystem = () => profile.PayToSystem(amount);
        var receiveFromSystem = () => profile.ReceiveFromSystem(amount);

        depositWallet.Should().Throw<InvalidInputException>();
        spendAvailable.Should().Throw<InvalidInputException>();
        increaseSystemBalance.Should().Throw<InvalidInputException>();
        decreaseSystemBalance.Should().Throw<InvalidInputException>();
        payToSystem.Should().Throw<InvalidInputException>();
        receiveFromSystem.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetMinAllowedBalance_ValidValue_UpdatesMinimum()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid());

        profile.SetMinAllowedBalance(-10.25m);

        profile.MinAllowedBalance.Should().Be(-10.25m);
    }

    [Fact]
    public void SetMinAllowedBalance_Zero_UpdatesMinimum()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid(), -10m);

        profile.SetMinAllowedBalance(0m);

        profile.MinAllowedBalance.Should().Be(0m);
    }

    [Fact]
    public void SetMinAllowedBalance_PositiveValue_Throws()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid(), -10m);

        var act = () => profile.SetMinAllowedBalance(0.01m);

        act.Should().Throw<InvalidInputException>();
        profile.MinAllowedBalance.Should().Be(-10m);
    }

    [Fact]
    public void SetMinAllowedBalance_MoreThanTwoDecimals_Throws()
    {
        var profile = UserFinancialProfile.Create(Guid.NewGuid(), -10m);

        var act = () => profile.SetMinAllowedBalance(-1.123m);

        act.Should().Throw<InvalidInputException>();
        profile.MinAllowedBalance.Should().Be(-10m);
    }

    [Fact]
    public void Create_PositiveMinAllowedBalance_Throws()
    {
        var act = () => UserFinancialProfile.Create(Guid.NewGuid(), 0.01m);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Create_MinAllowedBalanceWithMoreThanTwoDecimals_Throws()
    {
        var act = () => UserFinancialProfile.Create(Guid.NewGuid(), -1.123m);

        act.Should().Throw<InvalidInputException>();
    }
}
