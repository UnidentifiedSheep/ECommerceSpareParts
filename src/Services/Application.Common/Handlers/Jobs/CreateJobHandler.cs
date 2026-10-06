using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Attributes;

namespace Application.Common.Handlers.Jobs;

[Transactional]
[AutoSave]
public record CreateJobCommand(string SystemName, string InputState, int MaxAttempts)
	: ICommand<CreateJobResult>;

public record CreateJobResult(Guid JobId);

public class CreateJobHandler(IJobService jobService)
	: ICommandHandler<CreateJobCommand, CreateJobResult>
{
	public async Task<CreateJobResult> Handle(
		CreateJobCommand request,
		CancellationToken cancellationToken)
	{
		var jobIds = await jobService.TryEnqueueJobsAsync(
			[new JobItem(request.SystemName, request.InputState, request.MaxAttempts)],
			cancellationToken);

		if (jobIds.Count != 1)
			throw new InvalidOperationException("The requested job was not enqueued.");

		return new CreateJobResult(jobIds[0]);
	}
}
