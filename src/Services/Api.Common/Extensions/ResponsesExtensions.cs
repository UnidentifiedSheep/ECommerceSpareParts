using Application.Common.Abstractions;
using Application.Common.Attributes;
using Core.Attributes;

namespace Api.Common.Extensions;

public static class ResponsesExtensions
{
    public static TBuilder HasErrorFlow<TBuilder>(this TBuilder builder, Type flowType) 
        where TBuilder : IEndpointConventionBuilder
    {
        if (builder is not RouteHandlerBuilder routeBuilder) return builder;
        var commandFlow = FindCommandFlow(flowType);
        
        if (commandFlow == null) return builder;
        
        var flowSteps = GetFlowSteps(commandFlow);
        flowSteps.Add(flowType);

        foreach (var step in flowSteps)
        {
            var exceptionTypes = GetExceptionTypes(step);
            foreach (var exceptionType in exceptionTypes)
            {
                var statusCode = exceptionType.GetStatusCode();
                var exceptionExample = exceptionType.GetExceptionExample();
                routeBuilder.Produces(statusCode, exceptionExample.GetType());
            }
        }


        return builder;
    }

    private static Type[] GetExceptionTypes(Type step)
    {
        var exceptionTypes = step
            .GetCustomAttributes(inherit: true)
            .Where(a =>
                    a is ExceptionTypeAttribute || // обычный
                    (a.GetType().IsGenericType &&
                     a.GetType().GetGenericTypeDefinition() == typeof(ExceptionTypeAttribute<>)) // generic
            )
            .Select(a =>
            {
                // Обычный атрибут
                if (a is ExceptionTypeAttribute nonGeneric)
                    return nonGeneric.ExceptionType;

                // Generic: получаем TException
                if (a.GetType().IsGenericType &&
                    a.GetType().GetGenericTypeDefinition() == typeof(ExceptionTypeAttribute<>))
                {
                    return a.GetType().GetGenericArguments()[0]; // TException
                }

                return null!;
            })
            .Distinct()
            .ToArray();
        return exceptionTypes;
    }
    
    private static List<Type> GetFlowSteps(Type commandFlowType)
    {
        var attrs = commandFlowType.GetCustomAttributes(inherit: false);

        var steps = attrs
            .Where(a => a.GetType().IsGenericType &&
                        a.GetType().GetGenericTypeDefinition() == typeof(FlowStepAttribute<>))
            .Select(a =>
            {
                var type = a.GetType();
                var requestType = type.GetProperty("RequestType")?.GetValue(a) as Type;
                return requestType;
            })
            .Where(t => t != null)
            .Select(t => t!)
            .Distinct()
            .ToList();

        return steps;
    }
    
    private static Type? FindCommandFlow(Type commandType)
    {
        var assembly = commandType.Assembly;

        var flowType = assembly.GetTypes()
            .FirstOrDefault(t =>
                t is { IsAbstract: false, IsInterface: false, BaseType.IsGenericType: true } &&
                t.BaseType.GetGenericTypeDefinition() == typeof(CommandFlow<>) &&
                t.BaseType.GetGenericArguments()[0] == commandType
            );

        return flowType;
    }
}