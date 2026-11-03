using System.Data;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Extensions;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
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

public class SetToZeroContentHandler(IRepository<StorageContent, int> repository, TimeProvider timeProvider)
	: ICommandHandler<SetToZeroContentCommand>
{
	public async Task<Unit> Handle(SetToZeroContentCommand request, CancellationToken cancellationToken)
	{
		var content = await repository.GetWithActiveLocksForUpdateAsync(
			request.ContentId, cancellationToken: cancellationToken);

		content.ValidateVersion(request.RowVersion);

		content.SetCount(0, StorageMovementType.StorageContentDeletion, timeProvider.GetUtcNow().UtcDateTime);

		return Unit.Value;
	}
}
