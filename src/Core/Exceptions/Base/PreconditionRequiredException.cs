using System.Net;

namespace Exceptions.Base;

public class PreconditionRequiredException : BaseValuedException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.PreconditionRequired;
    public PreconditionRequiredException(string message) : base(message)
    {
    }

    public PreconditionRequiredException(string message, object relatedData) : base(message, relatedData)
    {
    }

}