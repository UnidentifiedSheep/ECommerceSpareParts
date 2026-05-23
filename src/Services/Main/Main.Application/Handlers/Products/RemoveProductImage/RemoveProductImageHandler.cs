using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Products;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Setting;
using MediatR;

namespace Main.Application.Handlers.Products.RemoveProductImage;

[Transactional, AutoSave]
public record RemoveProductImageCommand(int ProductId, string ImagePath) : ICommand;

public class RemoveProductImageHandler(
    IS3StorageService s3Storage,
    IUnitOfWork unitOfWork,
    ISettingsService settingsService,
    IRepository<ProductImage, (int, string)> repository,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<RemoveProductImageCommand>
{
    public async Task<Unit> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        var applicationSettings =
            (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken)).Data;

        var imageEntity = await repository.GetById((request.ProductId, request.ImagePath), cancellationToken)
                          ?? throw new ProductImageNotFoundException(request.ProductId, request.ImagePath);
        
        unitOfWork.Remove(imageEntity);

        var objectKey = GetObjectKey(imageEntity, applicationSettings);
        await s3Storage.DeleteFileAsync(applicationSettings.ImageBucketName, objectKey);

        integrationEventScope.Add(new ProductUpdatedEvent
        {
            Id = request.ProductId
        });
        return Unit.Value;
    }

    private static string GetObjectKey(ProductImage image, GlobalApplicationSettingData settings)
    {
        if (!string.IsNullOrWhiteSpace(image.Description))
            return image.Description;

        if (!Uri.TryCreate(image.Path, UriKind.Absolute, out var uri))
            return image.Path.TrimStart('/');

        var bucketPrefix = $"/{settings.ImageBucketName}/";
        var path = uri.AbsolutePath;
        var bucketIndex = path.IndexOf(bucketPrefix, StringComparison.OrdinalIgnoreCase);

        return bucketIndex >= 0
            ? path[(bucketIndex + bucketPrefix.Length)..]
            : path.TrimStart('/');
    }
}
