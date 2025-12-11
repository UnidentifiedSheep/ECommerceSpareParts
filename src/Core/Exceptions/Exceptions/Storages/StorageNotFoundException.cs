using Exceptions.Base;
using Core.Attributes;

namespace Exceptions.Exceptions.Storages;

public class StorageNotFoundException : NotFoundException
{
    public StorageNotFoundException(string name) : base("Склад не найден", new { Name = name })
    {
    }
}