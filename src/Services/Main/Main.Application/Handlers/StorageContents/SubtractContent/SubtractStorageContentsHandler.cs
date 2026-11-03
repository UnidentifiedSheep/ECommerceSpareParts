using System.Data;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Storage;
using Main.Enums;

namespace Main.Application.Handlers.StorageContents.SubtractContent;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record SubtractStorageContentsCommand(
	IReadOnlyCollection<ISubtractStorageContentItem> Items,
	StorageMovementType MovementType) : ICommand<SubtractStorageContentsResult>
{
	public SubtractStorageContentsCommand(
		int storageContentId,
		int count,
		StorageMovementType movementType) : this(
		[new SubtractStorageContentItem(storageContentId, count)],
		movementType)
	{
	}
}

public record SubtractStorageContentsResult(IReadOnlyList<StorageLot> Contents);

public class SubtractStorageContentsHandler(IStorageContentService storageContentService)
	: ICommandHandler<SubtractStorageContentsCommand, SubtractStorageContentsResult>
{
	public async Task<SubtractStorageContentsResult> Handle(
		SubtractStorageContentsCommand request,
		CancellationToken cancellationToken) =>
		new(await storageContentService.SubtractAsync(request.Items, request.MovementType, cancellationToken));
}
