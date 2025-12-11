using Core.Abstractions;

namespace Exceptions.Base.Examples;

public class ConflictExample : BaseExceptionExample
{
    public ConflictExample()
    {
        Status = 409;
        Title = "Conflict";
        Detail = "The request could not be completed due to a conflict with the current state of the target resource.";
    }
}