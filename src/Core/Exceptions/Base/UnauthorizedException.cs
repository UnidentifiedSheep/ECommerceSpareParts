using System.Net;
using Abstractions.Interfaces.Exceptions;

namespace Exceptions.Base;

public class UnauthorizedException(string? message) : Exception(message), IStatusCode
{
    public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
}