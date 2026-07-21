using Domain.Extensions;

namespace Domain.CommonEntities;

public class UniqJob : Job
{
    protected UniqJob() {}
    protected UniqJob(
        string naturalKey,
        string systemName, 
        string initialState,
        int maxAttempts) : base(systemName, initialState, maxAttempts)
    {
        SetNaturalKey(naturalKey);
    }

    public string NaturalKey { get; private set; } = string.Empty;
    
    public static UniqJob Create(
        string naturalKey,
        string systemName,
        string initialState,
        int maxAttempts = 3)
        => new(naturalKey, systemName, initialState, maxAttempts);

    private void SetNaturalKey(string naturalKey)
    {
        NaturalKey = naturalKey.TrimSafe()
            .EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("Natural key cannot be null or empty."));
    }
}