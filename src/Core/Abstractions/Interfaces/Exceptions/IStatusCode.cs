using System.Net;

namespace Abstractions.Interfaces.Exceptions;

public interface IStatusCode
{
    HttpStatusCode StatusCode { get; }
}