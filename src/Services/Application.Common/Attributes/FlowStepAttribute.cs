using MediatR;

namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class FlowStepAttribute<TRequest> : Attribute
{
    public Type RequestType { get; }
    public Type ResponseType { get; }

    public FlowStepAttribute()
    {
        RequestType = typeof(TRequest);
        if (!typeof(IRequest).IsAssignableFrom(RequestType) && !RequestType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
        {
            throw new ArgumentException(
                $"Type {RequestType.FullName} must implement MediatR.IRequest or IRequest<T>.", 
                nameof(RequestType)
            );
        }
        ResponseType = ResolveResponseType(RequestType);
    }

    private static Type ResolveResponseType(Type requestType)
    {
        var iface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

        return iface?.GetGenericArguments()[0] ?? typeof(Unit);
    }
}