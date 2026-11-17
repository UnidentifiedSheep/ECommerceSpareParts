using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Analytics;
using Main.Entities.Analytics.Exceptions;

namespace Main.Modules.Analytics.Application.Commands;

[Transactional]
[AutoSave]
public sealed record SetAnalyticsRunPinnedCommand(Guid RunId, bool IsPinned)
	: ICommand<SetAnalyticsRunPinnedResult>;

public sealed record SetAnalyticsRunPinnedResult(Guid RunId);

public sealed class SetAnalyticsRunPinnedHandler(IRepository<AnalyticsRun, Guid> repository)
	: ICommandHandler<SetAnalyticsRunPinnedCommand, SetAnalyticsRunPinnedResult>
{
	public async Task<SetAnalyticsRunPinnedResult> Handle(
		SetAnalyticsRunPinnedCommand request,
		CancellationToken cancellationToken)
	{
		var run = await repository.GetById(request.RunId, cancellationToken) ??
			throw new AnalyticsRunNotFoundException(request.RunId);

		if (request.IsPinned)
			run.Pin();
		else
			run.Unpin();

		return new SetAnalyticsRunPinnedResult(run.Id);
	}
}
