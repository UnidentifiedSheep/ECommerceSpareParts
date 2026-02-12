using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.Sales;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;

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