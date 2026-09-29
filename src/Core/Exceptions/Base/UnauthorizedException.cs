using System.Net;
using Exceptions.Interfaces;

namespace Exceptions.Base;

public class UnauthorizedException(string? message) : Exception(message), IStatusCode
{
	public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
}
