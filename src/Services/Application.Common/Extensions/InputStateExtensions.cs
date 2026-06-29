using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.NamedObject;

namespace Application.Common.Extensions;

public static class InputStateExtensions
{
    public static void ValidateState(
        this LrtNamedObjectBase lrt,
        string state)
    {
        if (JsonSerializer.Deserialize(state, lrt.InputType) is not IInputState inputState)
            throw new InvalidOperationException("Invalid input state");
        
        inputState.ValidateState();
    }
}