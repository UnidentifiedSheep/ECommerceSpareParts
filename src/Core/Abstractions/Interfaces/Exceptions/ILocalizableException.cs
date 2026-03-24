namespace Abstractions.Interfaces.Exceptions;

public interface ILocalizableException
{
    string MessageKey { get; }
}