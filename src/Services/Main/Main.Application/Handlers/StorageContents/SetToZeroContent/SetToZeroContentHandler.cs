using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Event;
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

public class SetToZeroContentHandler(
    IRepository<StorageContent, int> repository
) : ICommandHandler<SetToZeroContentCommand>
{
    public async Task<Unit> Handle(SetToZeroContentCommand request, CancellationToken cancellationToken)
    {
        var content = await repository.GetById(request.ContentId, cancellationToken)
                      ?? throw new StorageContentNotFoundException(request.ContentId);

        content.ValidateVersion(request.RowVersion);

        content.IncreaseCount(-content.Count, StorageMovementType.StorageContentDeletion);

        return Unit.Value;
    }
}