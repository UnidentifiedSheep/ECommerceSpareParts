using Exceptions;
using FluentAssertions;
using Main.Entities.User;

namespace Tests.Domain.User;

public class UserVehicleAggregateTests
{
	[Fact]
	public void AddUserVehicle_ValidData_AddsVehicle()
	{
		var userId = Guid.NewGuid();
		var vehicles = new List<UserVehicle>();
		var vehicleId = Guid.NewGuid();

		vehicles.AddUserVehicle(
			userId,
			
			vehicleId,
			"a123bc",
			"vin",
			"comment");

		vehicles.Should().ContainSingle();
		var vehicle = vehicles.Single();
		vehicle.VehicleId.Should().Be(vehicleId);
		vehicle.PlateNumber.Should().Be("A123BC");
		vehicle.Vin.Should().Be("VIN");
		vehicle.Comment.Should().Be("comment");
	}

	[Fact]
	public void AddUserVehicle_DuplicatePlateNumber_Throws()
	{
		var userId = Guid.NewGuid();
		var vehicles = new List<UserVehicle>();
		vehicles.AddUserVehicle(
			userId,
			Guid.NewGuid(),
			"a123bc");

		var act = () => vehicles.AddUserVehicle(
			userId,
			Guid.NewGuid(),
			" A123BC ");

		act.Should().Throw<InvalidInputException>();
		vehicles.Should().ContainSingle();
	}

	[Fact]
	public void AddUserVehicle_DuplicateVin_Throws()
	{
		var userId = Guid.NewGuid();
		var vehicles = new List<UserVehicle>();
		vehicles.AddUserVehicle(
			userId,
			
			Guid.NewGuid(),
			"a123bc",
			"vin");

		var act = () => vehicles.AddUserVehicle(
			userId,
			
			Guid.NewGuid(),
			"b321ca",
			" VIN ");

		act.Should().Throw<InvalidInputException>();
		vehicles.Should().ContainSingle();
	}

	[Fact]
	public void AddUserVehicle_NullVin_AllowsMultipleVehicles()
	{
		var userId = Guid.NewGuid();
		var vehicles = new List<UserVehicle>();

		vehicles.AddUserVehicle(
			userId,
			Guid.NewGuid(),
			"a123bc");
		vehicles.AddUserVehicle(
			userId,
			Guid.NewGuid(),
			"b321ca");

		vehicles.Should().HaveCount(2);
	}
}
