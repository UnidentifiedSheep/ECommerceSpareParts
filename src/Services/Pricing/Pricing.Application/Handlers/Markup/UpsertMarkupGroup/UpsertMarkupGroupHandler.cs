using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Analytics;
using Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities;
using Pricing.Entities.Exceptions.Markup;

namespace Pricing.Application.Handlers.Markup.UpsertMarkupGroup;

[Transactional(IsolationLevel.Serializable, 20, 3, TransactionErrorKind.UniqueConstraintViolation)]
[AutoSave]
public record UpsertMarkupGroupCommand(UpsertMarkupGroupDto MarkupGroup, Guid? SourceMessageId = null)
	: ICommand;

public class UpsertMarkupGroupHandler(
	IRepository<MarkupGroup, int> repository,
	IReadRepository<MarkupGroup, int> readRepository,
	IUnitOfWork unitOfWork,
	IIntegrationEventScope integrationEvents) : ICommandHandler<UpsertMarkupGroupCommand>
{
	public async Task<Unit> Handle(UpsertMarkupGroupCommand request, CancellationToken cancellationToken)
	{
		if (request.MarkupGroup.Id is null && request.SourceMessageId is { } messageId &&
			await readRepository.Query.AnyAsync(x => x.SourceMessageId == messageId, cancellationToken))
			return Unit.Value;

		var ranges = request.MarkupGroup.Ranges
			.Select(x => MarkupRange.Create(x.RangeStart, x.RangeEnd, x.Markup))
			.ToList();

		if (request.MarkupGroup.Id is null)
			await unitOfWork.AddAsync(
				MarkupGroup.Create(
					request.MarkupGroup.Name,
					request.MarkupGroup.CurrencyId,
					ranges,
					request.SourceMessageId),
				cancellationToken);
		else
		{
			var criteria = Criteria<MarkupGroup>
				.New()
				.Where(x => x.Id == request.MarkupGroup.Id)
				.Include(x => x.MarkupRanges)
				.Track()
				.Build();

			var group = await repository.FirstOrDefaultAsync(criteria, cancellationToken) ??
				throw new MarkupGroupNotFoundException(request.MarkupGroup.Id.Value);

			group.Update(request.MarkupGroup.Name, request.MarkupGroup.CurrencyId, ranges);
		}

		integrationEvents.Add(new MarkupRangesRefreshRequestedEvent());
		return Unit.Value;
	}
}
