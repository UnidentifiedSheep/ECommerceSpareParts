namespace Abstractions.Interfaces;

public interface IConcurrencyValidator<T>
{
    bool IsValid(T item, string concurrencyCode, out string validCode);
}