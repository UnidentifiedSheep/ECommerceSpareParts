using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using MediatR;

namespace Main.Application.Handlers.ArticleImages.MapImgsToArticle;

[Transactional]
[ExceptionType<ArticleNotFoundException>]
public record MapImgsToArticleCommand(int ArticleId, IEnumerable<IFile> Images) : ICommand;

public class MapImgsToArticleHandler(IS3StorageService s3Storage, IUnitOfWork unitOfWork, IMediator mediator, 
    DbDataValidatorBase dbValidator) 
    : ICommandHandler<MapImgsToArticleCommand, Unit>
{
    public async Task<Unit> Handle(MapImgsToArticleCommand request, CancellationToken cancellationToken)
    {
        //Validating if article exists
        var plan = new ValidationPlan().EnsureArticleExists(request.ArticleId);
        await dbValidator.Validate(plan, true, true, cancellationToken);
        
        var keys = new HashSet<string>();
        var toAdd = new List<ArticleImage>();
        try
        {
            foreach (var img in request.Images)
            {
                await using var stream = img.OpenReadStream();
                var path = $"imgs/articles/{request.ArticleId}_{Guid.NewGuid()}{img.Extension}";
                var key = await s3Storage.UploadFileAsync(Global.ImageBucketName, stream, path, "image/webp");
                keys.Add(key);
                toAdd.Add(new ArticleImage
                {
                    ArticleId = request.ArticleId,
                    Path = $"{Global.ServiceUrl}/{Global.ImageBucketName}/{path}"
                });
            }
            await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            foreach (var key in keys)
                await s3Storage.DeleteFileAsync(Global.ImageBucketName, key);
            throw;
        }
        await mediator.Publish(new ArticleUpdatedNotification(request.ArticleId), cancellationToken);
        return Unit.Value;
    }
}