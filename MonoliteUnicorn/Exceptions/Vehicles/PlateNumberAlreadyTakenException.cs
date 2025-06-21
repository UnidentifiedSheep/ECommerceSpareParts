using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Vehicles;

public class PlateNumberAlreadyTakenException(string? plateNumber) : BadRequestException($"Гос номер '{plateNumber}' уже используется.")
{
    
}