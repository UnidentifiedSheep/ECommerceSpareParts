using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Application.Notifications;
using Main.Entities;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ArticleImages.MapImgsToArticle;

[AutoSave]
[Transactional]
public record MapImgsToProductCommand(int ProductId, IEnumerable<IFile> Images) : ICommand;

public class MapImgsToProductHandler(IS3StorageService s3Storage, IUnitOfWork unitOfWork, IMediator mediator)
    : ICommandHandler<MapImgsToProductCommand, Unit>
{
    public async Task<Unit> Handle(MapImgsToProductCommand request, CancellationToken cancellationToken)
    {
        var keys = new HashSet<string>();
        var toAdd = new List<ProductImage>();
        try
        {
            foreach (var img in request.Images)
            {
                await using var stream = img.OpenReadStream();
                var path = $"imgs/articles/{request.ProductId}_{Guid.NewGuid()}{img.Extension}";
                var key = await s3Storage.UploadFileAsync(Global.ImageBucketName,
                    stream, path, "image/webp");
                keys.Add(key);
                toAdd.Add(ProductImage.Create(
                    productId: request.ProductId, 
                    path: $"{Global.ServiceUrl}/{Global.ImageBucketName}/{path}", 
                    description: key));
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

        await mediator.Publish(new ArticleUpdatedNotification(request.ProductId), cancellationToken);
        return Unit.Value;
    }
}