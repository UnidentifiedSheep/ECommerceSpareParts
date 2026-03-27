using System.Net;

namespace Exceptions.Base;

public class PreconditionRequiredException : BaseValuedException
{
    public PreconditionRequiredException(string? message, object relatedData) : base(message, relatedData)
    {
    }

    public override HttpStatusCode StatusCode => HttpStatusCode.PreconditionRequired;
}