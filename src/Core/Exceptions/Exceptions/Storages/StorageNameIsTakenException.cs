using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class StorageNameIsTakenException : BadRequestException
{
    public StorageNameIsTakenException(string name) : base($"Склад с таким именем уже существует", new { Name = name })
    {
    }
}