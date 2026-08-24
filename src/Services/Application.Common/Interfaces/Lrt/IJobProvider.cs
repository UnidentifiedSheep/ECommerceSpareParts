using Domain.CommonEntities.Job;

namespace Application.Common.Interfaces.Lrt;

public interface IJobProvider<TLrt, in TInputState>
    where TLrt : ILrtNamedObject<TInputState>
    where TInputState : IInputState
{
    Job Create(TInputState inputState, int maxAttempts = 3);
}
