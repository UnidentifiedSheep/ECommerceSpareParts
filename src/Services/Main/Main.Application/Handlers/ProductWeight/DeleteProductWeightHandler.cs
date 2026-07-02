using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Products;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.Handlers.ProductWeight.DeleteProductWeight;

[AutoSave]
[Transactional]
public record DeleteProductWeightCommand(int ProductId) : ICommand;

public class DeleteProductWeightHandler(
    IRepository<Entities.Product.ProductWeight, int> repository,
    IUnitOfWork unitOfWork,
    IDomainEventScope domainEventScope
    ) : ICommandHandler<DeleteProductWeightCommand>
{
    public async Task<Unit> Handle(DeleteProductWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await repository.GetById(request.ProductId, cancellationToken);
        unitOfWork.Remove(weight);
        domainEventScope.Add(new ProductWeightUpdatedNotification(request.ProductId));
        return Unit.Value;
    }
}