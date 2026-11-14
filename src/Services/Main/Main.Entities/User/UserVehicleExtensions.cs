using Exceptions;
using Main.Entities.Exceptions;

namespace Main.Entities.User;

public static class UserVehicleExtensions
{
	public static UserVehicle AddUserVehicle(
		this ICollection<UserVehicle> vehicles,
		Guid userId,
		Guid vehicleId,
		string plateNumber,
		string? vin = null,
		string? comment = null)
	{
		var normalizedPlateNumber = UserVehicle.NormalizePlateNumber(plateNumber);
		if (vehicles.Any(x => x.PlateNumber == normalizedPlateNumber))
			throw new InvalidInputException(UserHaveDuplicateVehiclePlateNumberMessage.Instance);

		var normalizedVin = UserVehicle.NormalizeVin(vin);
		if (normalizedVin != null && vehicles.Any(x => x.Vin == normalizedVin))
			throw new InvalidInputException(UserHaveDuplicateVehicleVinCodeMessage.Instance);

		var vehicle = UserVehicle.Create(
			userId,
			vehicleId,
			plateNumber,
			vin,
			comment);
		vehicles.Add(vehicle);
		return vehicle;
	}
}
