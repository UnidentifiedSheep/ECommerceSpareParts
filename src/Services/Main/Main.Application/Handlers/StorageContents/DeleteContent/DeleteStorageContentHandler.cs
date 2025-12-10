using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Base;
using Exceptions.Exceptions.Storages;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.DeleteContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
[ExceptionType<StorageContentNotFoundException>]
[ExceptionType<ConcurrencyCodeMismatchException>]
public record DeleteStorageContentCommand(int ContentId, string ConcurrencyCode, Guid UserId) : ICommand;

public class DeleteStorageContentHandler(
    DbDataValidatorBase dbValidator,
    IStorageContentRepository storageContentRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IConcurrencyValidator<StorageContent> concurrencyValidator,
    IArticlesService articlesService) : ICommandHandler<DeleteStorageContentCommand>
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

        await mediator.Publish(new ArticleUpdatedNotification(content.ArticleId), cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(string concurrencyCode, Guid userId, StorageContent content,
        CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan().EnsureUserExists(userId);
        await dbValidator.Validate(plan, true, true, cancellationToken);

        if (!concurrencyValidator.IsValid(content, concurrencyCode, out var validCode))
            throw new ConcurrencyCodeMismatchException(concurrencyCode, validCode);
    }
}