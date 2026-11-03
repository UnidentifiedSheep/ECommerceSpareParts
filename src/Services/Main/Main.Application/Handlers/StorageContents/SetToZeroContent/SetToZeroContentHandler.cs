using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Extensions;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Storage;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.StorageContents.SetToZeroContent;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record SetToZeroContentCommand(int ContentId, uint RowVersion) : ICommand;

public class SetToZeroContentHandler(IRepository<StorageContent, int> repository, IStorageContentService storageContentService)
	: ICommandHandler<SetToZeroContentCommand>
{
	public async Task<Unit> Handle(SetToZeroContentCommand request, CancellationToken cancellationToken)
	{
		var content = await repository.EnsureExistForUpdateAsync(request.ContentId,
			id => new StorageContentNotFoundException(id), ct: cancellationToken);

		content.ValidateVersion(request.RowVersion);

		await storageContentService.EnsureCanSetQuantitiesAsync([new StorageContentQuantityChange(content, 0)], cancellationToken);

		content.IncreaseCount(-content.Count, StorageMovementType.StorageContentDeletion);

		return Unit.Value;
	}
}
