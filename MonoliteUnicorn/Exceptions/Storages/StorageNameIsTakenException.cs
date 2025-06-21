using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageNameIsTakenException(string name) : BadRequestException($"Склад с именем '{name}' уже существует")
{
    
}