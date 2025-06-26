using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageContentCountCantBeNegativeException : BadRequestException
{
    public StorageContentCountCantBeNegativeException(int key) : base($"Элемент ID={key}. Количество не может быть отрицательным")
    {
    }
    public StorageContentCountCantBeNegativeException() : base($"Количество не может быть отрицательным")
    {
    }
}