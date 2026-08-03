namespace Domain.CommonEntities.Job;

public sealed class JobStep : Job
{
    private JobStep()
    {
    }

    private JobStep(
        Guid multiStepJobId,
        string systemName,
        string initialState,
        int maxAttempts)
        : base(
            systemName,
            initialState,
            maxAttempts)
    {
        MultiStepJobId = multiStepJobId;
    }

    public Guid MultiStepJobId { get; private set; }

    public MultiStepJob MultiStepJob { get; private set; } = null!;

    private readonly List<JobStepDependency> _dependencies = [];

    public IReadOnlyList<JobStepDependency> Dependencies => _dependencies;

    internal static JobStep Create(
        Guid multiStepJobId,
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        return new JobStep(
            multiStepJobId,
            systemName,
            initialState,
            maxAttempts);
    }

    internal void AddDependency(JobStep dependsOn)
    {
        ArgumentNullException.ThrowIfNull(dependsOn);

        if (Id == dependsOn.Id)
            throw new InvalidOperationException("Job step cannot depend on itself.");

        if (_dependencies.Any(x => x.DependsOnStepId == dependsOn.Id))
            return;

        _dependencies.Add(JobStepDependency.Create(Id, dependsOn.Id));
    }
}