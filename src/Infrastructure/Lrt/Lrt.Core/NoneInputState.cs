using Application.Common.Interfaces;

namespace Application.Common;

public record NoneInputState : IInputState
{
	public const string Json = "{}";

	public void ValidateState()
	{
	}
}
