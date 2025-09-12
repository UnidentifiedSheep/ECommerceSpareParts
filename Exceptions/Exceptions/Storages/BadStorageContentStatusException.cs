using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Storages;

public class BadStorageContentStatusException(string status) : 
    BadRequestException($"Нельзя удалять позиции со склада со статусом '{status}'", new { Status = status })
{
    
}