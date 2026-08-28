using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.ProductWeight;

[AutoSave]
[Transactional]
public record DeleteProductWeightCommand(int ProductId) : ICommand;

public class DeleteProductWeightHandler(
    IRepository<Entities.Product.ProductWeight, int> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteProductWeightCommand>
{
    public async Task<Unit> Handle(DeleteProductWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await repository.GetById(request.ProductId, cancellationToken)
            ?? throw new ProductWeightNotFoundException(request.ProductId);
        unitOfWork.Remove(weight);
        return Unit.Value;
    }
}