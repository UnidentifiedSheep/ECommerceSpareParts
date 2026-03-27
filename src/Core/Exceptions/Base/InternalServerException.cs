using System.Net;
using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base;

public class InternalServerException : Exception, IStatusCode
{
    public InternalServerException(string message) : base(message)
    {
    }

    public InternalServerException(string message, string details) : base(message)
    {
        Details = details;
    }

    public string? Details { get; }
    public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
}