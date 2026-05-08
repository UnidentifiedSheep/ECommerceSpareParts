using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Articles;
using Main.Abstractions.Models.Settings;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.Products.MapImgsToProduct;

[AutoSave]
[Transactional]
public record MapImgsToProductCommand(int ProductId, IEnumerable<IFile> Images) : ICommand;

public class MapImgsToProductHandler(
    IS3StorageService s3Storage,
    IUnitOfWork unitOfWork,
    ISettingsService settingsService,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<MapImgsToProductCommand, Unit>
{
    public async Task<Unit> Handle(MapImgsToProductCommand request, CancellationToken cancellationToken)
    {
        var keys = new HashSet<string>();
        var toAdd = new List<ProductImage>();
        var applicationSettings =
            (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken)).Data;
        try
        {
            foreach (var img in request.Images)
            {
                await using var stream = img.OpenReadStream();
                var path = $"imgs/articles/{request.ProductId}_{Guid.NewGuid()}{img.Extension}";
                var key = await s3Storage.UploadFileAsync(applicationSettings.ImageBucketName,
                    stream, path, "image/webp");
                keys.Add(key);
                toAdd.Add(ProductImage.Create(
                    request.ProductId,
                    $"{applicationSettings.ServiceUrl}/{applicationSettings.ImageBucketName}/{path}",
                    key));
            }

            await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            foreach (var key in keys)
                await s3Storage.DeleteFileAsync(applicationSettings.ImageBucketName, key);
            throw;
        }

        integrationEventScope.Add(new ProductUpdatedEvent { Id = request.ProductId });
        return Unit.Value;
    }
}