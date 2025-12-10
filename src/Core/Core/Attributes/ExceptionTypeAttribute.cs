namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ExceptionTypeAttribute : Attribute
{
    public Type ExceptionType { get; }

    public ExceptionTypeAttribute(Type exceptionType)
    {
        if (!typeof(Exception).IsAssignableFrom(exceptionType))
            throw new ArgumentException("Type must derive from Exception", nameof(exceptionType));

        ExceptionType = exceptionType;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ExceptionTypeAttribute<TException> : Attribute
    where TException : Exception
{
    public Type ExceptionType => typeof(TException);
}