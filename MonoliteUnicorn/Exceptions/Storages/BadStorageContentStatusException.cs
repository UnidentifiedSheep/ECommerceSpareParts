using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class BadStorageContentStatusException(string status) : 
    BadRequestException($"Нельзя удалять позиции со склада со статусом '{status}'", new { Status = status })
{
    
}