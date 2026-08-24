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

    private SingleRunJob(
        string naturalKey,
        string systemName,
        string initialState,
        int maxAttempts)
        : base(
            systemName,
            initialState,
            maxAttempts,
            naturalKey)
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

    public static SingleRunJob CreateUnique(
        string naturalKey,
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        ArgumentNullException.ThrowIfNull(naturalKey);

        return new SingleRunJob(
            naturalKey,
            systemName,
            initialState,
            maxAttempts);
    }
}
