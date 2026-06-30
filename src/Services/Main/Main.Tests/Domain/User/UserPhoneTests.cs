using Exceptions;
using FluentAssertions;
using Main.Entities.User;
using Main.Enums;

namespace Tests.Domain.User;

public class UserPhoneTests
{
    [Fact]
    public void Create_ValidData_NormalizesPhone()
    {
        var userId = Guid.NewGuid();

        var phone = UserPhone.Create(userId, " +90 (555) 123-45-67 ", PhoneType.Mobile);

        phone.UserId.Should().Be(userId);
        phone.PhoneNumber.Should().Be("+90 (555) 123-45-67");
        phone.NormalizedPhone.Should().Be("905551234567");
        phone.PhoneType.Should().Be(PhoneType.Mobile);
        phone.GetId().Should().Be("905551234567");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abcdefg")]
    [InlineData("123456")]
    [InlineData("1234567890123456")]
    public void Create_InvalidPhone_Throws(string phoneNumber)
    {
        var act = () => UserPhone.Create(Guid.NewGuid(), phoneNumber, PhoneType.Unknown);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Confirm_True_SetsConfirmedAt()
    {
        var phone = UserPhone.Create(Guid.NewGuid(), "+1 555 123 4567", PhoneType.Mobile);

        phone.Confirm();

        phone.Confirmed.Should().BeTrue();
        phone.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_False_ClearsConfirmedAt()
    {
        var phone = UserPhone.Create(Guid.NewGuid(), "+1 555 123 4567", PhoneType.Mobile);
        phone.Confirm();

        phone.Confirm(false);

        phone.Confirmed.Should().BeFalse();
        phone.ConfirmedAt.Should().BeNull();
    }

    [Fact]
    public void ChangeType_UpdatesPhoneType()
    {
        var phone = UserPhone.Create(Guid.NewGuid(), "+1 555 123 4567", PhoneType.Mobile);

        phone.ChangeType(PhoneType.Work);

        phone.PhoneType.Should().Be(PhoneType.Work);
    }

    [Fact]
    public void MakePrimary_UpdatesPrimaryFlag()
    {
        var phone = UserPhone.Create(Guid.NewGuid(), "+1 555 123 4567", PhoneType.Mobile);

        phone.MakePrimary();

        phone.IsPrimary.Should().BeTrue();
    }
}
