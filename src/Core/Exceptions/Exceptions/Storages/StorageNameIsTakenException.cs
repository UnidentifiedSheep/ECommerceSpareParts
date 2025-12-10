using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class StorageNameIsTakenException : BadRequestException
{
    [ExampleExceptionValues(false,"Пример_ЦЕНТРАЛЬНЫЙ")]
    public StorageNameIsTakenException(string name) : base($"Склад с таким именем уже существует", new { Name = name })
    {
    }
}