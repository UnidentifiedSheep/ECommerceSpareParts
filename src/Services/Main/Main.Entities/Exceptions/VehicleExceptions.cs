using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class PlateNumberAlreadyTakenException(string plateNumber) : LocalizedBadRequestException(
	new UserVehiclePlateNumberAlreadyTakenMessage().WithPlateNumber(plateNumber),
	new
	{
		PlateNumber = plateNumber
	});

public class VinCodeAlreadyTakenException(string vinCode) : LocalizedBadRequestException(
	new UserVehicleVinCodeAlreadyTakenMessage().WithVinCode(vinCode),
	new
	{
		VinCode = vinCode
	});
