using Core.Abstractions;

namespace Exceptions.Base.Examples;

public class BadRequestExample : BaseExceptionExample
{
    public BadRequestExample()
    {
        Status = 400;
        Title = "Bad Request";
        Detail = "Something is wrong with your request.";
    }
}