using Core.Abstractions;

namespace Exceptions.Base.Examples;

public class UnauthorizedAccessExample : BaseExceptionExample
{
    public UnauthorizedAccessExample()
    {
        Status = 401;
        Title = "Unauthorized";
        Detail = "You are not authorized to access this resource.";
    }
}