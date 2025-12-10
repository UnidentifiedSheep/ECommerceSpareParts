using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Vehicles;

public class PlateNumberAlreadyTakenException : BadRequestException
{
    [ExampleExceptionValues(false,"ПР0990ИМЕР")]
    public PlateNumberAlreadyTakenException(string? plateNumber) : base($"Гос номер уже используется.", 
        new { PlateNumber = plateNumber })
    {
    }
}