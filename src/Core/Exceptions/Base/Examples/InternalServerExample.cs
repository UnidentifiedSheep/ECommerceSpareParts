using Core.Abstractions;

namespace Exceptions.Base.Examples;

public class InternalServerExample : BaseExceptionExample
{
    public InternalServerExample()
    {
        Status = 500;
        Title = "Internal Server Error";
        Detail = "Something went wrong on our end.";
    }
}