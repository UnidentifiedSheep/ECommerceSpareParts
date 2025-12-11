using Core.Abstractions;

namespace Exceptions.Base.Examples;

public class PreconditionRequiredExample : BaseExceptionExample
{
    public PreconditionRequiredExample()
    {
        Status = 428;
        Title = "Precondition Required";
        Detail = "The origin server requires the request to be conditional.";
    }
}