using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions.Sales;
using Main.Entities.Sale;

namespace Main.Application.Handlers.Sales.DeleteSale;

[AutoSave]
[Transactional]
public record DeleteSaleCommand(Guid SaleId) : ICommand<DeleteSaleResult>;

public record DeleteSaleResult(Sale Sale);

public class DeleteSaleHandler(
    IRepository<Sale, Guid> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteSaleCommand, DeleteSaleResult>
{
    public async Task<DeleteSaleResult> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await repository.GetById(request.SaleId, cancellationToken)
                   ?? throw new SaleNotFoundException(request.SaleId);
        unitOfWork.Remove(sale);
        return new DeleteSaleResult(sale);
    }
}