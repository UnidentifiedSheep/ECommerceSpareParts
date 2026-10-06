using System.Net;

namespace Exceptions.Base;

public class GoneException : BaseValuedException
{
	public GoneException(string? message) : base(message)
	{
	}

	public GoneException(string? message, object relatedData) : base(message, relatedData)
	{
	}

	public override HttpStatusCode StatusCode => HttpStatusCode.Gone;
}
