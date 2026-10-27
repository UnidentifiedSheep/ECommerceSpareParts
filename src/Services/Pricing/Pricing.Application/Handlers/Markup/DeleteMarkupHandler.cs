using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
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
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteMarkupCommand>
{
	public async Task<Unit> Handle(DeleteMarkupCommand request, CancellationToken cancellationToken)
	{
		var group = await repository.GetById(request.Id, cancellationToken)
			?? throw new MarkupGroupNotFoundException(request.Id);

		var settings = await settingsService.GetOrDefault<PricingSetting>(directRead: true, cancellationToken);
		var currentId = settings.Data.SelectedMarkupId ?? await readRepository.Query
			.Where(x => x.SourceMessageId != null)
			.OrderByDescending(x => x.CreatedAt)
			.ThenByDescending(x => x.Id)
			.Select(x => (int?)x.Id)
			.FirstOrDefaultAsync(cancellationToken);

		if (currentId == group.Id)
			throw new CurrenMarkupGroupCanNotBeDeletedException();

		unitOfWork.Remove(group);
		return Unit.Value;
	}
}
