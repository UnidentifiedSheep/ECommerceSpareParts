using Abstractions.Models.Repository;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Entities;
using Main.Entities.Sale;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface ISaleRepository
{
    Task<Sale?> GetSaleForUpdate(string saleId, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<SaleContent>> GetSaleContentsForUpdate(
        string saleId,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SaleContentDetail>> GetSaleContentDetailsForUpdate(
        IEnumerable<int> saleContentIds,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Sale>> GetSales(
        QueryOptions<Sale, GetSalesOptionsData> options,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SaleContent>> GetSaleContent(
        Guid saleId,
        bool track = true,
        CancellationToken cancellationToken = default);
}