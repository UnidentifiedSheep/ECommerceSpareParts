using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Interfaces.Repositories;
using Main.Application.Notifications;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ArticleWeight.DeleteArticleWeight;

[AutoSave]
[Transactional]
public record DeleteProductWeightCommand(int ProductId) : ICommand;

public class DeleteProductWeightHandler(
    IRepository<ProductWeight, int> repository,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : ICommandHandler<DeleteProductWeightCommand>
{
    public async Task<Unit> Handle(DeleteProductWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await repository.GetById(request.ProductId, cancellationToken);
        unitOfWork.Remove(weight);
        
        await publisher.Publish(new ArticleWeightUpdatedNotification(request.ProductId), cancellationToken);
        return Unit.Value;
    }
}