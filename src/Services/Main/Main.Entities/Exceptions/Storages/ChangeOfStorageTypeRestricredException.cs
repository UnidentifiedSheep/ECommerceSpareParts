using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Storages;

public class ChangeOfStorageTypeRestrictedException() : BadRequestException(null), ILocalizableException
{
    public string MessageKey => "storage.type.change.restricted";
    public object[]? Arguments => null;
}