using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageContentCountCantBeNegativeException : BadRequestException
{
    public StorageContentCountCantBeNegativeException(int id) : base($"Количество не может быть отрицательным", new { Id = id })
    {
    }
    public StorageContentCountCantBeNegativeException() : base($"Количество не может быть отрицательным")
    {
    }
}