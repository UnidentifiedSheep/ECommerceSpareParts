namespace Core.Exceptions;

public class PreconditionRequiredException : BaseValuedException
{
    public PreconditionRequiredException(string message) : base(message) { }
    public PreconditionRequiredException(string message, object relatedData) : base(message, relatedData) { }
}