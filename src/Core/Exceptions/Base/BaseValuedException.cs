using System.Net;
using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base;

public abstract class BaseValuedException : Exception, IValuedException, IStatusCode
{
    private readonly object? _errorValues;

    protected BaseValuedException(string? message, object key) : base(message)
    {
        _errorValues = key;
    }

    protected BaseValuedException(string? message) : base(message)
    {
    }

    public abstract HttpStatusCode StatusCode { get; }

    public object? GetErrorValues()
    {
        return _errorValues;
    }
}