using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Products;
using Enums;
using Main.Entities.DomainEvents.Product;
using MediatR;

namespace Main.Application.Handlers.ProductWeight.SetProductWeight;

[AutoSave]
[Transactional]
public record SetProductWeightCommand(
    int ProductId,
    decimal Weight,
    WeightUnit Unit
) : ICommand;

public class SetProductWeightHandler(
    IDomainEventScope domainEventScope,
    IRepository<Entities.Product.ProductWeight, int> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<SetProductWeightCommand>
{
    public async Task<Unit> Handle(SetProductWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await repository.GetById(request.ProductId, cancellationToken);

        if (weight == null)
        {
            weight = Entities.Product.ProductWeight.Create(
                request.ProductId,
                request.Weight,
                request.Unit);
            await unitOfWork.AddAsync(weight, cancellationToken);
        }
        else { weight.Update(request.Weight, request.Unit); }

        domainEventScope.Add(new ProductWeightUpdatedDomainEvent(request.ProductId));
        return Unit.Value;
    }
}