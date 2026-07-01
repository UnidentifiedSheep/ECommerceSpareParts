using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class PlateNumberAlreadyTakenException(string plateNumber)
    : LocalizedBadRequestException(
        "user.vehicle.plate.number.already.taken",
        new { PlateNumber = plateNumber },
        [plateNumber]);

public class VinCodeAlreadyTakenException(string vinCode)
    : LocalizedBadRequestException(
        "user.vehicle.vin.code.already.taken",
        new { VinCode = vinCode },
        [vinCode]);