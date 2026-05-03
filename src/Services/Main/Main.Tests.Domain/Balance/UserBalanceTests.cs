using FluentAssertions;
using Main.Entities.User;

namespace Main.Tests.Domain.Balance;

public class UserBalanceTests
{
    [Fact]
    public void Create_InitialBalance_IsZero()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        balance.Balance.Should().Be(0);
    }

    [Fact]
    public void IncrementBalance_AddsValue()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        balance.IncrementBalance(10.5m);

        balance.Balance.Should().Be(10.5m);
    }

    [Fact]
    public void IncrementBalance_AllowsNegative()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        balance.IncrementBalance(10m);
        balance.IncrementBalance(-3m);

        balance.Balance.Should().Be(7m);
    }

    [Fact]
    public void IncrementBalance_MoreThanTwoDecimals_Throws()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        var act = () => balance.IncrementBalance(1.123m);

        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void Create_SetsCurrencyId()
    {
        var userId = Guid.NewGuid();

        var balance = UserBalance.Create(userId, 42);

        balance.CurrencyId.Should().Be(42);
        balance.UserId.Should().Be(userId);
    }
    
    [Fact]
    public void IncrementBalance_AllowsExactlyTwoDecimals()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        balance.IncrementBalance(1.23m);

        balance.Balance.Should().Be(1.23m);
    }
    
    [Fact]
    public void IncrementBalance_MultipleOperations_PrecisionStable()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        balance.IncrementBalance(0.10m);
        balance.IncrementBalance(0.20m);
        balance.IncrementBalance(0.30m);

        balance.Balance.Should().Be(0.60m);
    }
    
    [Fact]
    public void IncrementBalance_LargeValues_HandlesCorrectly()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        balance.IncrementBalance(1_000_000m);
        balance.IncrementBalance(2_000_000m);

        balance.Balance.Should().Be(3_000_000m);
    }
    
    [Fact]
    public void IncrementBalance_AllowsNegativeBalance()
    {
        var balance = UserBalance.Create(Guid.NewGuid(), 1);

        balance.IncrementBalance(-100m);

        balance.Balance.Should().Be(-100m);
    }
}