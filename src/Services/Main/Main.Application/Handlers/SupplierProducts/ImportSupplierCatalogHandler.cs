using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Contracts.Models.Supplier;
using Enums;
using Main.Entities.Product.Supplier;
using Main.Entities.Product.ValueObjects;
using MediatR;

namespace Main.Application.Handlers.SupplierProducts;
//TODO this should be implemented.
public record ImportSupplierCatalogCommand(
    Supplier Supplier,
    IReadOnlyCollection<ContractSupplierProductDto> Products
) : ICommand;

public class ImportSupplierCatalogHandler(
    IRepository<SupplierProduct, int> repository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<ImportSupplierCatalogCommand>
{
    public async Task<Unit> Handle(
        ImportSupplierCatalogCommand request, 
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task<Dictionary<SupplierProductKey, SupplierProduct>> GetExistingProducts(
        IEnumerable<ContractSupplierProductDto> products,
        CancellationToken cancellationToken)
    {
        var keys = products
            .Select(x => new SupplierProductKey(Sku.ToNormalized(x.Number), x.Brand))
            .ToHashSet();
        
        var producers = keys.Select(x => x.Producer).Distinct().ToList();
        var numbers = keys.Select(x => x.Sku).Distinct().ToList();
        
        var criteria = Criteria<SupplierProduct>.New()
            .Where(x => producers.Contains(x.Producer))
            .Where(x => numbers.Contains(x.Sku.NormalizedValue))
            .Track(false)
            .Build();
        
        throw new NotImplementedException();
    }

    private record struct SupplierProductKey(string Sku, string Producer);
}