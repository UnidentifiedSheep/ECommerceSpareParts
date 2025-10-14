namespace Exceptions.Base;

public class GroupedException : BaseValuedException
{
    public readonly IReadOnlyList<Exception> Exceptions;
    public GroupedException(IEnumerable<Exception> exceptions) : base("", exceptions)
    {
        Exceptions = new List<Exception>(exceptions);
    }
    
    public GroupedException(Exception exception) : base("", exception)
    {
        Exceptions = new List<Exception>([exception]);
    }
}