using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Exceptions;

public class InvalidInputException : BadRequestException, ILocalizableException
{
    public InvalidInputException(string key, object[]? arguments = null, string? message = null) : base(message)
    {
        MessageKey = key;
        Arguments = arguments;
    }

    public string MessageKey { get; }
    public object[]? Arguments { get; }
}