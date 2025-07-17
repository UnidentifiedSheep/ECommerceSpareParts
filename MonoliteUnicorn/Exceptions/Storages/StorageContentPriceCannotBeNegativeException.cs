using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageContentPriceCannotBeNegativeException : BadRequestException
{
    public StorageContentPriceCannotBeNegativeException(int id) : base($"Цена должна быть больше 0", new { Id = id })
    {
    }
    public StorageContentPriceCannotBeNegativeException() : base($"Цена должна быть больше 0")
    {
    }
}