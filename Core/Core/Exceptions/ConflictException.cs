namespace Core.Exceptions;

public class ConflictException : BaseValuedException
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, string details) : base(message)
    {
        Details = details;
    }
    public ConflictException(string message, object relatedData) : base(message, relatedData) { }

    public string? Details { get; }
}