using System.Text.Json;
using Application.Common.Interfaces.Lrt;

namespace Application.Common.Extensions;

public static class InputStateExtensions
{
	public static string ValidateState(this ILrtNamedObject lrt, string state)
	{
		if (JsonSerializer.Deserialize(state, lrt.InputType) is not IInputState inputState)
			throw new InvalidOperationException("Invalid input state");

		inputState.ValidateState();
		return JsonSerializer.Serialize(inputState, inputState.GetType());
	}
}
