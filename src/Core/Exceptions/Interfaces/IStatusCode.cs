using System.Net;

namespace Exceptions.Interfaces;

public interface IStatusCode
{
	HttpStatusCode StatusCode { get; }
}
