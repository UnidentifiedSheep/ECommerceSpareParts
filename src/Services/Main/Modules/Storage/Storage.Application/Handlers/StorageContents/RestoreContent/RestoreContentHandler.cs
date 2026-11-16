using System.Data;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Entities.Storage.Exceptions;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Application.Models;
using MediatR;

namespace Main.Modules.Storage.Application.Handlers.StorageContents.RestoreContent;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record RestoreContentCommand(
	IReadOnlyCollection<RestoreContentItem> ContentDetails,
	StorageMovementType MovementType) : ICommand;

public class RestoreContentHandler(IRepository<StorageContent, int> contentRepository)
	: ICommandHandler<RestoreContentCommand>
{
	public async Task<Unit> Handle(RestoreContentCommand request, CancellationToken cancellationToken)
	{
		var contentDetailsList = request.ContentDetails.ToList();
		var storageContentIds = contentDetailsList.Select(x => x.StorageContentId).Distinct().ToList();

		var storageContents = await contentRepository.EnsureExistsForUpdateAsync(
			storageContentIds,
			missing => new StorageContentNotFoundException(missing),
			Criteria<StorageContent>.New().OrderByAsc(content => content.Id),
			cancellationToken);

		foreach (var detail in contentDetailsList)
		{
			var content = storageContents[detail.StorageContentId];
			content.IncreaseCount(detail.Count, request.MovementType);
		}

		return Unit.Value;
	}
}
