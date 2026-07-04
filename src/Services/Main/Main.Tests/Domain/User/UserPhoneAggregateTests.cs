using Exceptions;
using FluentAssertions;
using Main.Enums;

namespace Tests.Domain.User;

public class UserPhoneAggregateTests
{
    [Fact]
    public void AddUserPhone_ValidData_AddsPhone()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");

        user.AddUserPhone(
            "+1 555 123 4567",
            PhoneType.Mobile,
            true,
            true);

        user.Phones.Should().ContainSingle();
        var phone = user.Phones.Single();
        phone.NormalizedPhone.Should().Be("15551234567");
        phone.IsPrimary.Should().BeTrue();
        phone.Confirmed.Should().BeTrue();
    }

    [Fact]
    public void AddUserPhone_DuplicateNormalizedPhone_Throws()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");
        user.AddUserPhone(
            "+1 555 123 4567",
            PhoneType.Mobile,
            false,
            false);

        var act = () => user.AddUserPhone(
            "1-555-123-4567",
            PhoneType.Work,
            false,
            false);

        act.Should().Throw<InvalidInputException>();
        user.Phones.Should().ContainSingle();
    }

    [Fact]
    public void AddUserPhone_SecondPrimaryPhone_Throws()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");
        user.AddUserPhone(
            "+1 555 123 4567",
            PhoneType.Mobile,
            true,
            false);

        var act = () => user.AddUserPhone(
            "+1 555 765 4321",
            PhoneType.Work,
            true,
            false);

        act.Should().Throw<InvalidInputException>();
        user.Phones.Should().ContainSingle();
    }

    [Fact]
    public void RemoveUserPhone_RemovesByNormalizedPhone()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");
        user.AddUserPhone(
            "+1 555 123 4567",
            PhoneType.Mobile,
            false,
            false);

        user.RemoveUserPhone("1-555-123-4567");

        user.Phones.Should().BeEmpty();
    }
}