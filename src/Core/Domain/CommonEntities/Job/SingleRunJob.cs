namespace Domain.CommonEntities.Job;

public sealed class SingleRunJob : Job
{
    private SingleRunJob()
    {
    }

    private SingleRunJob(
        string systemName,
        string initialState,
        int maxAttempts)
        : base(
            systemName,
            initialState,
            maxAttempts)
    {
    }

    public static SingleRunJob Create(
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        return new SingleRunJob(
            systemName,
            initialState,
            maxAttempts);
    }
}
