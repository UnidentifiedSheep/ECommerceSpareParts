using Core.Abstractions;

namespace Exceptions.Base.Examples;

public class NotFoundExample : BaseExceptionExample
{
    public NotFoundExample()
    {
        Status = 404;
        Title = "Not Found";
        Detail = "The requested resource could not be found.";
    }
}