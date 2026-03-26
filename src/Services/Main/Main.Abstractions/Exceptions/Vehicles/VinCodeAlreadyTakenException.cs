using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Vehicles;

public class VinCodeAlreadyTakenException : BadRequestException, ILocalizableException
{
    public string MessageKey => "user.vehicle.vin.code.already.taken";
    public object[]? Arguments { get; }
    public VinCodeAlreadyTakenException(string vinCode) : base(null, new { VinCode = vinCode })
    {
        Arguments = [vinCode];
    }
}