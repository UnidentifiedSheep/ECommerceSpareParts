using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Application.Common.Models.Options.S3;
using Attributes;
using Exceptions;
using Main.Application.Static;
using Main.Entities.Product;
using Main.Entities.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Products.MapImgsToProduct;

[AutoSave]
[Transactional]
public record MapImgsToProductCommand(int ProductId, IEnumerable<IFile> Images) : ICommand;

public class MapImgsToProductHandler(
    IS3StorageService s3Storage,
    IUnitOfWork unitOfWork,
    IOptions<S3BucketsOptions> bucketsOptions
    ) : ICommandHandler<MapImgsToProductCommand, Unit>
{
    public async Task<Unit> Handle(MapImgsToProductCommand request, CancellationToken cancellationToken)
    {
        var keys = new HashSet<string>();
        var toAdd = new List<ProductImage>();
        var opt = bucketsOptions.Value.Images;
        try
        {
            foreach (var img in request.Images)
            {
                var model = ProductImage.Create(
                    request.ProductId,
                    img.Extension);
                await using var stream = img.OpenReadStream();
                var key = await s3Storage.UploadFileAsync(
                    opt.Name,
                    stream,
                    model.StorageKey,
                    "image/webp");
                keys.Add(key);
                toAdd.Add(model);
            }

            await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            foreach (var key in keys) 
                await s3Storage.DeleteFileAsync(opt.Name, key);
            throw;
        }
        
        return Unit.Value;
    }
}
