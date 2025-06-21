using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageContentPriceCannotBeNegativeException(int key) : BadRequestException($"Элемент ID={key} Цена должна быть больше 0")
{
    
}