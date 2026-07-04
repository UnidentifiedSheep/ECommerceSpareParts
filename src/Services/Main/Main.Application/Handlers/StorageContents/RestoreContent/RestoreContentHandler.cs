using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

[AutoSave]
[Transactional(
    IsolationLevel.ReadCommitted,
    20,
    2)]
public record RestoreContentCommand(
    IEnumerable<RestoreContentItem> ContentDetails,
    StorageMovementType MovementType
) : ICommand;

public class RestoreContentHandler(
    IStorageContentRepository contentRepository
) : ICommandHandler<RestoreContentCommand>
{
    public async Task<Unit> Handle(RestoreContentCommand request, CancellationToken cancellationToken)
    {
        var contentDetailsList = request.ContentDetails.ToList();
        var storageContentIds = contentDetailsList
            .Select(x => x.StorageContentId)
            .Distinct()
            .ToList();
        
        var storageContents = await contentRepository
            .EnsureExistsForUpdateAsync(
                storageContentIds,
                nf => new StorageContentNotFoundException(nf),
                cancellationToken);

        foreach (var detail in contentDetailsList)
        {
            var content = storageContents[detail.StorageContentId];
            content.IncreaseCount(detail.Count, request.MovementType);
        }

        return Unit.Value;
    }
}