using Exceptions;
using FluentAssertions;
using Main.Entities.User;

namespace Tests.Domain.User;

public class UserVehicleTests
{
    [Fact]
    public void Create_ValidData_SetsNormalizedValues()
    {
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var vehicle = UserVehicle.Create(
            userId,
            vehicleId,
            "  a123bc  ",
            "  x123vin  ",
            "  test comment  ");

        vehicle.UserId.Should().Be(userId);
        vehicle.VehicleId.Should().Be(vehicleId);
        vehicle.PlateNumber.Should().Be("A123BC");
        vehicle.Vin.Should().Be("X123VIN");
        vehicle.Comment.Should().Be("test comment");
    }

    [Fact]
    public void Create_EmptyVehicleId_Throws()
    {
        var act = () => UserVehicle.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "A123BC");

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyPlateNumber_Throws(string plateNumber)
    {
        var act = () => UserVehicle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            plateNumber);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetVin_WhiteSpace_SetsNull()
    {
        var vehicle = UserVehicle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "A123BC",
            "VIN");

        vehicle.SetVin("   ");

        vehicle.Vin.Should().BeNull();
    }

    [Fact]
    public void SetComment_WhiteSpace_SetsNull()
    {
        var vehicle = UserVehicle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "A123BC",
            null,
            "comment");

        vehicle.SetComment("   ");

        vehicle.Comment.Should().BeNull();
    }

    [Fact]
    public void SetPlateNumber_TooLong_Throws()
    {
        var vehicle = UserVehicle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "A123BC");
        var plate = new string('A', UserVehicle.MaxPlateNumberLength + 1);

        var act = () => vehicle.SetPlateNumber(plate);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetVin_TooLong_Throws()
    {
        var vehicle = UserVehicle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "A123BC");
        var vin = new string('A', UserVehicle.MaxVinLength + 1);

        var act = () => vehicle.SetVin(vin);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetComment_TooLong_Throws()
    {
        var vehicle = UserVehicle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "A123BC");
        var comment = new string('A', UserVehicle.MaxCommentLength + 1);

        var act = () => vehicle.SetComment(comment);

        act.Should().Throw<InvalidInputException>();
    }
}