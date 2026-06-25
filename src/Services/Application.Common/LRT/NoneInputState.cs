using Application.Common.Interfaces.Lrt;

namespace Application.Common.LRT;

public record NoneInputState : IInputState
{
    public static string GetAndValidateState(string jsonState)
    {
        return jsonState;
    }
}