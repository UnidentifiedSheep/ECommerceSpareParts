using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Storages;

public class StorageNameIsTakenException(string name) : 
    BadRequestException($"Склад с именем '{name}' уже существует", new { Name = name })
{
    
}