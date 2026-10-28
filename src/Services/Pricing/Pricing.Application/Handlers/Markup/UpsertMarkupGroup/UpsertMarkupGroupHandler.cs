using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Analytics;
using Enums;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities;
using Pricing.Entities.Exceptions.Markup;

namespace Pricing.Application.Handlers.Markup.UpsertMarkupGroup;

[Transactional(IsolationLevel.Serializable, 20, 3, TransactionErrorKind.UniqueConstraintViolation)]
[AutoSave]
public record UpsertMarkupGroupCommand(UpsertMarkupGroupDto MarkupGroup, Guid? SourceMessageId = null)
	: ICommand<UpsertMarkupGroupResult>;

public record UpsertMarkupGroupResult(int Id);

public class UpsertMarkupGroupHandler(
	IRepository<MarkupGroup, int> repository,
	IReadRepository<MarkupGroup, int> readRepository,
	IUnitOfWork unitOfWork,
	IIntegrationEventScope integrationEvents) : ICommandHandler<UpsertMarkupGroupCommand, UpsertMarkupGroupResult>
{
	public async Task<UpsertMarkupGroupResult> Handle(UpsertMarkupGroupCommand request, CancellationToken cancellationToken)
	{
		if (request.MarkupGroup.Id is null && request.SourceMessageId is { } messageId)
		{
			var existingId = await readRepository.Query
				.Where(x => x.SourceMessageId == messageId)
				.Select(x => (int?)x.Id)
				.SingleOrDefaultAsync(cancellationToken);
			if (existingId is { } id)
				return new UpsertMarkupGroupResult(id);
		}

		var ranges = request.MarkupGroup.Ranges
			.Select(x => MarkupRange.Create(x.RangeStart, x.RangeEnd, x.Markup))
			.ToList();

		MarkupGroup group;
		if (request.MarkupGroup.Id is null)
		{
			group = MarkupGroup.Create(
					request.MarkupGroup.Name,
					request.MarkupGroup.CurrencyId,
					ranges,
					request.SourceMessageId);
			await unitOfWork.AddAsync(group, cancellationToken);
		}
		else
		{
			var criteria = Criteria<MarkupGroup>
				.New()
				.Where(x => x.Id == request.MarkupGroup.Id)
				.Include(x => x.MarkupRanges)
				.Track()
				.Build();

			group = await repository.FirstOrDefaultAsync(criteria, cancellationToken) ??
				throw new MarkupGroupNotFoundException(request.MarkupGroup.Id.Value);

			group.Update(request.MarkupGroup.Name, request.MarkupGroup.CurrencyId, ranges);
		}

		integrationEvents.Add(new MarkupRangesRefreshRequestedEvent());
		await unitOfWork.SaveChangesAsync(cancellationToken);
		return new UpsertMarkupGroupResult(group.Id);
	}
}
