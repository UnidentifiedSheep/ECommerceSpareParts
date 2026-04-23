using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Notifications;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductSizes.DeleteProductSizes;

[AutoSave]
[Transactional]
public record DeleteArticleSizesCommand(int ProductId) : ICommand;

public class DeleteProductSizesHandler(
    IRepository<ProductSize, int> repository,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : ICommandHandler<DeleteArticleSizesCommand>
{
    public async Task<Unit> Handle(DeleteArticleSizesCommand request, CancellationToken cancellationToken)
    {
        var sizes = await repository.GetById(request.ProductId, cancellationToken)
                    ?? throw new ProductSizesNotFoundException(request.ProductId);
        
        unitOfWork.Remove(sizes);

        await publisher.Publish(new ArticleSizeUpdatedNotification(request.ProductId), cancellationToken);
        return Unit.Value;
    }
}