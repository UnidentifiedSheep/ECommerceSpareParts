using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Articles;
using Enums;
using MediatR;

namespace Main.Application.Handlers.ProductWeight.SetProductWeight;

[AutoSave]
[Transactional]
public record SetProductWeightCommand(int ProductId, decimal Weight, WeightUnit Unit) : ICommand;

public class SetProductWeightHandler(
    IIntegrationEventScope integrationEventScope,
    IRepository<Entities.Product.ProductWeight, int> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetProductWeightCommand>
{
    public async Task<Unit> Handle(SetProductWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await repository.GetById(request.ProductId, cancellationToken);

        if (weight == null)
        {
            weight = Entities.Product.ProductWeight.Create(request.ProductId, request.Weight, request.Unit);
            await unitOfWork.AddAsync(weight, cancellationToken);
        }
        else
        {
            weight.Update(request.Weight, request.Unit);
        }

        integrationEventScope.Add(new ProductWeightUpdatedEvent
        {
            ProductId = request.ProductId
        });
        return Unit.Value;
    }
}