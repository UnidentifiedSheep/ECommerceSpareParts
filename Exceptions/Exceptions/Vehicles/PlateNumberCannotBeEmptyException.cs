using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Vehicles;

public class PlateNumberCannotBeEmptyException(string? plateNumber) : BadRequestException("Номер автомобиля не может быть пустым", new { PlateNumber = plateNumber })
{
    
}