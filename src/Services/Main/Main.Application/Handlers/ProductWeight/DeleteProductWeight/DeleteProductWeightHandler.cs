using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.Handlers.ProductWeight.DeleteProductWeight;

[AutoSave]
[Transactional]
public record DeleteProductWeightCommand(int ProductId) : ICommand;

public class DeleteProductWeightHandler(
    IRepository<Entities.Product.ProductWeight, int> repository,
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