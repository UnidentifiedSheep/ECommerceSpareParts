using System.Net;

namespace Exceptions.Base;

public class NotFoundException : BaseValuedException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, object relatedData) : base(message, relatedData)
    {
    }

}