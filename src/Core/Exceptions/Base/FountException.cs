namespace Exceptions.Base;

public class FoundException : BaseValuedException
{
    public FoundException(string message) : base(message)
    {
    }

    public FoundException(string message, object relatedData) : base(message, relatedData)
    {
    }
}