using Exceptions.Base;

namespace Exceptions.Exceptions.Vehicles;

public class PlateNumberAlreadyTakenException(string? plateNumber)
    : BadRequestException($"Гос номер '{plateNumber}' уже используется.")
{
}