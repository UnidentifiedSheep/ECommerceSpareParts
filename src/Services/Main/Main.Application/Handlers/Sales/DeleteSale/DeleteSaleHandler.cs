using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Exceptions.Sales;
using Main.Entities.Sale;

namespace Main.Application.Handlers.Sales.DeleteSale;

[Transactional]
public record DeleteSaleCommand(string SaleId) : ICommand<DeleteSaleResult>;

public record DeleteSaleResult(Sale Sale);

public class DeleteSaleHandler(ISaleRepository saleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteSaleCommand, DeleteSaleResult>
{
    public async Task<DeleteSaleResult> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetSaleForUpdate(request.SaleId, true, cancellationToken)
                   ?? throw new SaleNotFoundException(request.SaleId);
        unitOfWork.Remove(sale);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeleteSaleResult(sale);
    }
}