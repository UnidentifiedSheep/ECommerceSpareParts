using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class StorageNameIsTakenException(string name) :
    BadRequestException($"Склад с именем '{name}' уже существует", new { Name = name })
{
}