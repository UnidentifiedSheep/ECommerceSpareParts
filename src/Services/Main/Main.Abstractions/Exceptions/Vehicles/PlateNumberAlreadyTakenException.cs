using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Vehicles;

public class PlateNumberAlreadyTakenException : BadRequestException, ILocalizableException
{
    public PlateNumberAlreadyTakenException(string plateNumber) : base(null,
        new { PlateNumber = plateNumber })
    {
        Arguments = [plateNumber];
    }

    public string MessageKey => "user.vehicle.plate.number.already.taken";
    public object[]? Arguments { get; }
}