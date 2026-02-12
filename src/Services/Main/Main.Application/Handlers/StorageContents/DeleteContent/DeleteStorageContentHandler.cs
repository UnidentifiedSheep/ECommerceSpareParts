using System.Data;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Base;
using Exceptions.Exceptions.Storages;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Notifications;
using Main.Entities;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.DeleteContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteStorageContentCommand(int ContentId, string ConcurrencyCode, Guid UserId) : ICommand;

public class DeleteStorageContentHandler(IStorageContentRepository storageContentRepository,
    IUnitOfWork unitOfWork, IMediator mediator, IConcurrencyValidator<StorageContent> concurrencyValidator,
    IArticlesService articlesService) : ICommandHandler<DeleteStorageContentCommand>
{
    public async Task<Unit> Handle(DeleteStorageContentCommand request, CancellationToken cancellationToken)
    {
        var id = request.ContentId;
        var userId = request.UserId;
        var content =
            await storageContentRepository.GetStorageContentForUpdateAsync(id, null, null, true, cancellationToken)
            ?? throw new StorageContentNotFoundException(id);
        
        if (!concurrencyValidator.IsValid(content, request.ConcurrencyCode, out var validCode))
            throw new ConcurrencyCodeMismatchException(request.ConcurrencyCode, validCode);

        var storageMovement = content.Adapt<StorageMovement>()
            .SetActionType(StorageMovementType.StorageContentDeletion);
        storageMovement.Count *= -1;
        storageMovement.WhoMoved = userId;

        await unitOfWork.AddAsync(storageMovement, cancellationToken);
        unitOfWork.Remove(content);
        await articlesService.UpdateArticlesCount(new Dictionary<int, int> { [content.ArticleId] = -content.Count },
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticleUpdatedNotification(content.ArticleId), cancellationToken);
        return Unit.Value;
    }
}