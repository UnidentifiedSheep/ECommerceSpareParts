using Application.Common.Interfaces.Lrt;

namespace Application.Common.LRT;

public record NoneInputState : IInputState
{
    public void ValidateState() { }
}