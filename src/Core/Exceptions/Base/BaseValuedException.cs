using Core.Interfaces.Exceptions;

namespace Exceptions.Base;

public class BaseValuedException : Exception, IValuedException
{
    private readonly object? _errorValues;

    protected BaseValuedException(string message, object key) : base(message)
    {
        _errorValues = key;
    }

    protected BaseValuedException(string message) : base(message)
    {
    }

    public object? GetErrorValues()
    {
        return _errorValues;
    }
}