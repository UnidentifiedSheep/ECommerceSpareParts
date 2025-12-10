using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Base;
using Exceptions.Exceptions.Storages;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.MoveContentToOtherStorage;

[Transactional(IsolationLevel.ReadCommitted, 0, 2)]
[ExceptionType<StorageNotFoundException>]
[ExceptionType<ConcurrencyCodeMismatchException>]
public record MoveContentToOtherStorageCommand(IEnumerable<MoveStorageContentDto> Movements, Guid MovedBy) : ICommand;

public class MoveContentToOtherStorageHandler(IStorageContentService storageContentService, 
    DbDataValidatorBase dbValidator, IConcurrencyValidator<StorageContent> concurrencyValidator, IUnitOfWork unitOfWork) 
    : ICommandHandler<MoveContentToOtherStorageCommand>
{
    public async Task<Unit> Handle(MoveContentToOtherStorageCommand request, CancellationToken cancellationToken)
    {
        var ids = new HashSet<int>();
        var storageNames = new HashSet<string>();
        foreach (var movement in request.Movements)
        {
            ids.Add(movement.StorageContentId);
            storageNames.Add(movement.NewStorageName);
        }

        await ValidateData(storageNames, cancellationToken);
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

    private async Task ValidateData(IEnumerable<string> storageNames, CancellationToken ct)
    {
        var plan = new ValidationPlan().EnsureStorageExists(storageNames);
        await dbValidator.Validate(plan, true, true, ct);
    }
}