using Application.Common.Interfaces.Lrt;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;

namespace Tests.Stubs;

public sealed class JobScheduleTestInputState : IInputState
{
	public int Value { get; init; }

	public void ValidateState()
	{
	}
}

public sealed class JobScheduleTestLrt : ILrtNamedObject<JobScheduleTestInputState>
{
	public const string LrtName = "test-job-schedule-lrt";

	public string SystemName => LrtName;

	public ILocalizableMessage NameLocalizationMessage
		=> new LocalizableMessage("test-job-schedule-lrt.name");
	public ILocalizableMessage DescriptionLocalizationMessage
		=> new LocalizableMessage("test-job-schedule-lrt.description");

	public Type InputType => typeof(JobScheduleTestInputState);

	public Type StateType => typeof(JobScheduleTestInputState);

	public Task ExecuteAsync(
		Guid jobId,
		Guid leaseHolderId,
		CancellationToken cancellationToken = default) => Task.CompletedTask;
}
