using Exceptions;
using FluentAssertions;

namespace Tests.Domain.User;

public class UserVehicleAggregateTests
{
    [Fact]
    public void AddUserVehicle_ValidData_AddsVehicle()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");
        var vehicleId = Guid.NewGuid();

        user.AddUserVehicle(vehicleId, "a123bc", "vin", "comment");

        user.Vehicles.Should().ContainSingle();
        var vehicle = user.Vehicles.Single();
        vehicle.VehicleId.Should().Be(vehicleId);
        vehicle.PlateNumber.Should().Be("A123BC");
        vehicle.Vin.Should().Be("VIN");
        vehicle.Comment.Should().Be("comment");
    }

    [Fact]
    public void AddUserVehicle_DuplicatePlateNumber_Throws()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");
        user.AddUserVehicle(Guid.NewGuid(), "a123bc");

        var act = () => user.AddUserVehicle(Guid.NewGuid(), " A123BC ");

        act.Should().Throw<InvalidInputException>();
        user.Vehicles.Should().ContainSingle();
    }

    [Fact]
    public void AddUserVehicle_DuplicateVin_Throws()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");
        user.AddUserVehicle(Guid.NewGuid(), "a123bc", "vin");

        var act = () => user.AddUserVehicle(Guid.NewGuid(), "b321ca", " VIN ");

        act.Should().Throw<InvalidInputException>();
        user.Vehicles.Should().ContainSingle();
    }

    [Fact]
    public void AddUserVehicle_NullVin_AllowsMultipleVehicles()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");

        user.AddUserVehicle(Guid.NewGuid(), "a123bc");
        user.AddUserVehicle(Guid.NewGuid(), "b321ca");

        user.Vehicles.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveUserVehicle_RemovesByVehicleId()
    {
        var user = Main.Entities.User.User.Create("test-user", "hash");
        var vehicleId = Guid.NewGuid();
        user.AddUserVehicle(vehicleId, "a123bc");

        user.RemoveUserVehicle(vehicleId);

        user.Vehicles.Should().BeEmpty();
    }
}
