using System.Reflection;
using Application.Common.Interfaces.Lrt;

namespace Application.Common.Extensions;

public static class InputStateValidator
{
    public static string GetAndValidate(Type inputType, string jsonState)
    {
        var method = inputType.GetMethod(
            nameof(IInputState.GetAndValidateState),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string)]);

        if (method is null || method.ReturnType != typeof(string))
            throw new InvalidOperationException(
                $"Type {inputType.Name} must define public static string GetAndValidateState(string jsonState).");

        try
        {
            return (string)method.Invoke(null, [jsonState])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }
}