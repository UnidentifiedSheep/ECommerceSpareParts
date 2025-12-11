using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Vehicles;

public class VinCodeAlreadyTakenException : BadRequestException
{
    public VinCodeAlreadyTakenException(string? vinCode) : base($"Вин код занят.", new { VinCode = vinCode })
    {
    }
}