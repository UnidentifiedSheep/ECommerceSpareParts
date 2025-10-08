using System.Data;
using Main.Application.Extensions;
using Core.Attributes;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Base;
using Exceptions.Exceptions.Storages;
using Main.Application.Events;
using Main.Application.Interfaces;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.DeleteContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteStorageContentCommand(int ContentId, string ConcurrencyCode, Guid UserId) : ICommand;

public class DeleteStorageContentHandler(
    IStorageContentRepository storageContentRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IConcurrencyValidator<StorageContent> concurrencyValidator,
    IArticlesService articlesService,
    IUserRepository usersRepository) : ICommandHandler<DeleteStorageContentCommand>
{
    public async Task<Unit> Handle(DeleteStorageContentCommand request, CancellationToken cancellationToken)
    {
        var id = request.ContentId;
        var userId = request.UserId;
        var content =
            await storageContentRepository.GetStorageContentForUpdateAsync(id, null, null, true, cancellationToken)
            ?? throw new StorageContentNotFoundException(id);

        await ValidateData(request.ConcurrencyCode, userId, content, cancellationToken);

        var storageMovement = content.Adapt<StorageMovement>()
            .SetActionType(StorageMovementType.StorageContentDeletion);
        storageMovement.Count *= -1;
        storageMovement.WhoMoved = userId;

        await unitOfWork.AddAsync(storageMovement, cancellationToken);
        unitOfWork.Remove(content);
        await articlesService.UpdateArticlesCount(new Dictionary<int, int> { [content.ArticleId] = -content.Count },
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticleUpdatedEvent(content.ArticleId), cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(string concurrencyCode, Guid userId, StorageContent content,
        CancellationToken cancellationToken = default)
    {
        await usersRepository.EnsureUsersExists([userId], cancellationToken);

        if (!concurrencyValidator.IsValid(content, concurrencyCode, out var validCode))
            throw new ConcurrencyCodeMismatchException(concurrencyCode, validCode);
    }
}