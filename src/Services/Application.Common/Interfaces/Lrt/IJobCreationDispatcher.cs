using Domain.CommonEntities.Job;

namespace Application.Common.Interfaces.Lrt;

public interface IJobCreationDispatcher
{
    Job Create(
        string systemName,
        string inputState,
        int maxAttempts);
}
