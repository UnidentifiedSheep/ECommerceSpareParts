using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions.Products;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductCharacteristics.DeleteCharacteristics;

[AutoSave]
[Transactional]
public record DeleteCharacteristicsCommand(int ProductId, string Name) : ICommand;

public class DeleteCharacteristicsHandler(
    IRepository<ProductCharacteristic, (int, string)> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCharacteristicsCommand>
{
    public async Task<Unit> Handle(DeleteCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById((request.ProductId, request.Name))
                     ?? throw new ProductCharacteristicsNotFoundException(request.ProductId, request.Name);
        unitOfWork.Remove(entity);
        return Unit.Value;
    }
}