using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Vehicles;

public class PlateNumberAlreadyTakenException(string? plateNumber) : BadRequestException($"Гос номер '{plateNumber}' уже используется.")
{
    
}