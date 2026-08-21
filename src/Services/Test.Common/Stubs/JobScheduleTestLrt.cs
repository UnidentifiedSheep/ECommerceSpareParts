using Application.Common.Interfaces.Lrt;

namespace Tests.Stubs;

public sealed class JobScheduleTestInputState : IInputState
{
    public int Value { get; init; }
    public void ValidateState() { }
}

public sealed class JobScheduleTestLrt
    : ILrtNamedObject<JobScheduleTestInputState>
{
    public const string LrtName = "test-job-schedule-lrt";

    public string SystemName => LrtName;
    public string NameLocalizationKey => "test-job-schedule-lrt.name";
    public string DescriptionLocalizationKey => "test-job-schedule-lrt.description";
    public Type InputType => typeof(JobScheduleTestInputState);
    public Type StateType => typeof(JobScheduleTestInputState);

    public Task ExecuteAsync(
        Guid jobId,
        Guid leaseHolderId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
