using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Products;
using Main.Application.Static;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.Products;

[Transactional, AutoSave]
public record RemoveProductImageCommand(int ProductId, string ImagePath) : ICommand;

public class RemoveProductImageHandler(
    IS3StorageService s3Storage,
    IUnitOfWork unitOfWork,
    IRepository<ProductImage, (int, string)> repository,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<RemoveProductImageCommand>
{
    public async Task<Unit> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        var imagePath = NormalizeImagePath(request.ImagePath);

        var imageEntity = await repository.GetById((request.ProductId, imagePath), cancellationToken)
                          ?? throw new ProductImageNotFoundException(request.ProductId, imagePath);
        
        unitOfWork.Remove(imageEntity);

        var objectKey = GetObjectKey(imageEntity);
        await s3Storage.DeleteFileAsync(BucketNames.Images, objectKey);

        integrationEventScope.Add(new ProductUpdatedEvent
        {
            Id = request.ProductId
        });
        return Unit.Value;
    }

    private static string NormalizeImagePath(string imagePath)
    {
        var normalized = Uri.UnescapeDataString(imagePath).Trim();
        return normalized.Replace("http//", "http://", StringComparison.OrdinalIgnoreCase)
            .Replace("https//", "https://", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetObjectKey(ProductImage image)
    {
        if (!string.IsNullOrWhiteSpace(image.Description))
            return image.Description;

        if (!Uri.TryCreate(image.Path, UriKind.Absolute, out var uri))
            return image.Path.TrimStart('/');

        var bucketPrefix = $"/{BucketNames.Images}/";
        var path = uri.AbsolutePath;
        var bucketIndex = path.IndexOf(bucketPrefix, StringComparison.OrdinalIgnoreCase);

        return bucketIndex >= 0
            ? path[(bucketIndex + bucketPrefix.Length)..]
            : path.TrimStart('/');
    }
}
