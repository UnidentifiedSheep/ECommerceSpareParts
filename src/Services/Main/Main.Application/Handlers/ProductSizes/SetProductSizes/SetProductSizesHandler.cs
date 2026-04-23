using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Application.Notifications;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductSizes.SetProductSizes;

[AutoSave]
[Transactional]
public record SetProductSizesCommand(int ProductId, decimal Length, decimal Width, decimal Height, DimensionUnit Unit)
    : ICommand;

public class SetProductSizesHandler(
    IRepository<ProductSize, int> repository,
    IUnitOfWork unitOfWork,
    IPublisher mediator)
    : ICommandHandler<SetProductSizesCommand>
{
    public async Task<Unit> Handle(SetProductSizesCommand request, CancellationToken cancellationToken)
    {
        var height = request.Height;
        var length = request.Length;
        var width = request.Width;
        var unit = request.Unit;
        var sizes = await repository.GetById(request.ProductId, cancellationToken);

        if (sizes == null)
        {
            sizes = ProductSize.Create(request.ProductId, length, width, height, unit);
            await unitOfWork.AddAsync(sizes, cancellationToken);
        }
        
        sizes.SetLength(length);
        sizes.SetWidth(width);
        sizes.SetHeight(height);
        sizes.SetUnit(unit);

        //publish notification to invalidate cache
        await mediator.Publish(new ArticleSizeUpdatedNotification(request.ProductId), cancellationToken);

        return Unit.Value;
    }
}