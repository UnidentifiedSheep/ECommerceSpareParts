using Exceptions.Base;

namespace Exceptions.Exceptions.Vehicles;

public class PlateNumberAlreadyTakenException : BadRequestException
{
    public PlateNumberAlreadyTakenException(string? plateNumber) : base($"Гос номер уже используется.", 
        new { PlateNumber = plateNumber })
    {
    }
}