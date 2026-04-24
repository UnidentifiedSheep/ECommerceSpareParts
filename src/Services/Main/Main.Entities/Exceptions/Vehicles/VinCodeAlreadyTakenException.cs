using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Vehicles;

public class VinCodeAlreadyTakenException : BadRequestException, ILocalizableException
{
    public VinCodeAlreadyTakenException(string vinCode) : base(null, new { VinCode = vinCode })
    {
        Arguments = [vinCode];
    }

    public string MessageKey => "user.vehicle.vin.code.already.taken";
    public object[]? Arguments { get; }
}