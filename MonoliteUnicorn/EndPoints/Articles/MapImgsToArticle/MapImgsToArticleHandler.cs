using Core.Interface;
using Core.Services.S3;
using Core.StaticFunctions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.CacheService;

namespace MonoliteUnicorn.EndPoints.Articles.MapImgsToArticle;

public record MapImgsToArticleCommand(IFormFileCollection Imgs, int ArticleId) : ICommand;


public class MapImgsToArticleHandler(DContext context, IS3StorageService s3Storage, CacheQueue cacheQueue) : ICommandHandler<MapImgsToArticleCommand, Unit>
{
    public async Task<Unit> Handle(MapImgsToArticleCommand request, CancellationToken cancellationToken)
    {
        await context.EnsureArticlesExist(request.ArticleId, cancellationToken);
        var query = "INSERT INTO article_images (article_id, path) VALUES ";
        foreach (var img in request.Imgs)
        {
            await using var stream = ConvertTo.Webp(img.OpenReadStream());
            var path = $"imgs/articles/{request.ArticleId}_{Guid.NewGuid()}.webp";
            await s3Storage.UploadFileAsync(stream, path, "image/webp");
            await context.Database
                .ExecuteSqlRawAsync(query + $"({request.ArticleId}, '{Global.ServiceUrl}/{Global.S3BucketName}/{path}');", 
                    cancellationToken: cancellationToken);
        }

        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync([request.ArticleId]);
        });
        return Unit.Value;
    }
}