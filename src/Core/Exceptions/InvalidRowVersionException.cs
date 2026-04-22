using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Exceptions;

public class InvalidRowVersionException() : ConflictException(null), ILocalizableException
{
    public string MessageKey => "row.is.out.dated";
    public object[]? Arguments => null;
}