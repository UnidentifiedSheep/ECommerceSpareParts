using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Storages;

public class StorageContentCountCantBeNegativeException : BadRequestException
{
    public StorageContentCountCantBeNegativeException(int id) : base($"Количество не может быть отрицательным", new { Id = id })
    {
    }
    public StorageContentCountCantBeNegativeException() : base($"Количество не может быть отрицательным")
    {
    }
}