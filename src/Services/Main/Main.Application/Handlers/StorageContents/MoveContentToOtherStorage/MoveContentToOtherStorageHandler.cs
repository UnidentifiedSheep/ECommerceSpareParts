using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Base;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.Services;
using Main.Entities;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.MoveContentToOtherStorage;

[Transactional(IsolationLevel.ReadCommitted, 0, 2)]
public record MoveContentToOtherStorageCommand(IEnumerable<MoveStorageContentDto> Movements, Guid MovedBy) : ICommand;

public class MoveContentToOtherStorageHandler(IStorageContentService storageContentService, 
    IConcurrencyValidator<StorageContent> concurrencyValidator, IUnitOfWork unitOfWork) 
    : ICommandHandler<MoveContentToOtherStorageCommand>
{
    public async Task<Unit> Handle(MoveContentToOtherStorageCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Movements.Select(x => x.StorageContentId).ToHashSet();

        var storageContents = await storageContentService
            .GetStorageContentsForUpdate(ids, cancellationToken);
        
        var storageMovements = new List<StorageMovement>();
        
        foreach (var move in request.Movements)
        {
            var content = storageContents[move.StorageContentId];
            var concurCode = move.ConcurrencyCode;
            if (!concurrencyValidator.IsValid(content, concurCode, out var validCode))
                throw new ConcurrencyCodeMismatchException(concurCode, validCode);

            var movements = GetMovements(content, move.NewStorageName, request.MovedBy);
            storageMovements.AddRange(movements);
            content.StorageName = move.NewStorageName;
        }
        await unitOfWork.AddRangeAsync(storageMovements, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private List<StorageMovement> GetMovements(StorageContent content, string newStorageName, Guid whoMoved)
    {
        var movementF = content.Adapt<StorageMovement>()
            .SetActionType(StorageMovementType.StorageContentMovement);
        movementF.Count = -content.Count;
        movementF.WhoMoved = whoMoved;
        
        var movementS = content.Adapt<StorageMovement>()
            .SetActionType(StorageMovementType.StorageContentMovement);
        movementS.Count = content.Count;
        movementS.StorageName = newStorageName;
        movementS.WhoMoved = whoMoved;
        return [movementF, movementS];
    }
}