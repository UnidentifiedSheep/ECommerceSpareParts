using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Vehicles;

public class VinCodeAlreadyTakenException : BadRequestException
{
    [ExampleExceptionValues(false,"WBABW33426P-example")]
    public VinCodeAlreadyTakenException(string? vinCode) : base($"Вин код занят.", new { VinCode = vinCode })
    {
    }
}