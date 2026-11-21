using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Enums;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Application.Extensions;
using MediatR;

namespace Main.Modules.Storage.Application.Commands.StorageContents.SetToZeroContent;

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
			request.ContentId,
			cancellationToken: cancellationToken);

		content.ValidateVersion(request.RowVersion);

		content.SetCount(
			0,
			StorageMovementType.StorageContentDeletion,
			timeProvider.GetUtcNow().UtcDateTime);

		return Unit.Value;
	}
}
