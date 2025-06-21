using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageContentCountCantBeNegativeException(int key) : BadRequestException($"Элемент ID={key}. Количество не может быть отрицательным")
{
    
}