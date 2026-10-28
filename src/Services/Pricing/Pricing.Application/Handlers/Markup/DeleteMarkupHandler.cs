using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Analytics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Entities;
using Pricing.Entities.Exceptions.Markup;
using Pricing.Entities.Settings;

namespace Pricing.Application.Handlers.Markup;

[Transactional(IsolationLevel.Serializable, 20, 3)]
[AutoSave]
public record DeleteMarkupCommand(int Id) : ICommand;

public sealed class DeleteMarkupHandler(
	IRepository<MarkupGroup, int> repository,
	IReadRepository<MarkupGroup, int> readRepository,
	ISettingsService settingsService,
	IUnitOfWork unitOfWork,
	IIntegrationEventScope integrationEvents) : ICommandHandler<DeleteMarkupCommand>
{
	public async Task<Unit> Handle(DeleteMarkupCommand request, CancellationToken cancellationToken)
	{
		var group = await repository.GetById(request.Id, cancellationToken)
			?? throw new MarkupGroupNotFoundException(request.Id);

		var settings = await settingsService.GetOrDefault<PricingSetting>(directRead: true, cancellationToken);
		if (settings.Data.SelectedMarkupId == group.Id)
			throw new CurrenMarkupGroupCanNotBeDeletedException();

		if (!await readRepository.Query.AnyAsync(x => x.Id != group.Id, cancellationToken))
			throw new LastMarkupGroupCanNotBeDeletedException();

		unitOfWork.Remove(group);
		integrationEvents.Add(new MarkupRangesRefreshRequestedEvent());
		return Unit.Value;
	}
}
