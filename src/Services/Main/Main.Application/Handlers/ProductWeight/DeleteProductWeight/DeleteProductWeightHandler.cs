using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Products;
using MediatR;

namespace Main.Application.Handlers.ProductWeight.DeleteProductWeight;

[AutoSave]
[Transactional]
public record DeleteProductWeightCommand(int ProductId) : ICommand;

public class DeleteProductWeightHandler(
    IRepository<Entities.Product.ProductWeight, int> repository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope
)
    : ICommandHandler<DeleteProductWeightCommand>
{
    public async Task<Unit> Handle(DeleteProductWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await repository.GetById(request.ProductId, cancellationToken);
        unitOfWork.Remove(weight);

        integrationEventScope.Add(new ProductWeightUpdatedEvent { ProductId = request.ProductId });
        return Unit.Value;
    }
}