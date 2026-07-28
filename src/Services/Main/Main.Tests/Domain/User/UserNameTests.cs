using Exceptions;
using FluentAssertions;
using Main.Entities.User.ValueObjects;

namespace Tests.Domain.User;

public class UserNameTests
{
    [Fact]
    public void Constructor_WithValidValue_TrimsAndNormalizes()
    {
        var userName = new UserName("  Test-User  ");

        userName.Value.Should().Be("Test-User");
        userName.NormalizedValue.Should().Be("TEST-USER");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(36)]
    public void Constructor_WithBoundaryLength_CreatesUserName(int length)
    {
        var value = new string('a', length);

        var userName = new UserName(value);

        userName.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null, "login.must.not.be.empty")]
    [InlineData("", "login.must.not.be.empty")]
    [InlineData("   ", "login.must.not.be.empty")]
    [InlineData("user", "login.min.length.5")]
    [InlineData("valid user", "login.cannot.contain.spaces")]
    [InlineData("user@example", "login.cannot.contain.at.sign")]
    public void Constructor_WithInvalidValue_ThrowsExpectedInvalidInputException(
        string? value,
        string expectedMessageKey)
    {
        var action = () => new UserName(value!);

        action.Should()
            .Throw<InvalidInputException>()
            .Which.MessageKey.Should()
            .Be(expectedMessageKey);
    }

    [Fact]
    public void Constructor_WhenValueExceedsMaximumLength_ThrowsInvalidInputException()
    {
        var action = () => new UserName(new string('a', 37));

        action.Should()
            .Throw<InvalidInputException>()
            .Which.MessageKey.Should()
            .Be("login.max.length.36");
    }

    [Fact]
    public void ToNormalized_TrimsAndUsesInvariantUpperCase()
    {
        var normalized = UserName.ToNormalized("  test-user  ");

        normalized.Should().Be("TEST-USER");
    }

    [Fact]
    public void ImplicitConversions_PreserveValue()
    {
        UserName userName = "test-user";
        string value = userName;

        value.Should().Be("test-user");
    }
}
