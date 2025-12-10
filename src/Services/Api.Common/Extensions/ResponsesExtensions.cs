using Application.Common.Abstractions;
using Application.Common.Attributes;

namespace Api.Common.Extensions;

public static class ResponsesExtensions
{
    public static TBuilder ProducesErrorFlow<TBuilder, TFlow>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        var type = typeof(TFlow);
        var commandFlow = FindCommandFlow(type);
        
        if (commandFlow == null) return builder;
        
        var flowSteps = GetFlowSteps(commandFlow);
        
        builder.Add(endpoint =>
        {
        });


        return builder;
    }
    
    private static (Type request, Type response)[] GetFlowSteps(Type commandFlowType)
    {
        var attrs = commandFlowType.GetCustomAttributes(inherit: false);

        var steps = attrs
            .Where(a => a.GetType().IsGenericType &&
                        a.GetType().GetGenericTypeDefinition() == typeof(FlowStepAttribute<>))
            .Select(a =>
            {
                var type = a.GetType();
                var requestType = type.GetProperty("RequestType")?.GetValue(a) as Type;
                var responseType = type.GetProperty("ResponseType")?.GetValue(a) as Type;
                return (requestType!, responseType!);
            })
            .ToArray();

        return steps;
    }
    
    private static Type? FindCommandFlow(Type commandType)
    {
        var assembly = commandType.Assembly;

        var flowType = assembly.GetTypes()
            .FirstOrDefault(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                t.BaseType != null &&
                t.BaseType.IsGenericType &&
                t.BaseType.GetGenericTypeDefinition() == typeof(CommandFlow<>) &&
                t.BaseType.GetGenericArguments()[0] == commandType
            );

        return flowType;
    }
}