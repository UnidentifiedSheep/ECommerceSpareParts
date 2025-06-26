using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageContentPriceCannotBeNegativeException : BadRequestException
{
    public StorageContentPriceCannotBeNegativeException(int key) : base($"Элемент ID={key} Цена должна быть больше 0")
    {
    }
    public StorageContentPriceCannotBeNegativeException() : base($"Цена должна быть больше 0")
    {
    }
}