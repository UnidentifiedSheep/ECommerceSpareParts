using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface ISaleRepository
{
    Task<Sale?> GetSaleForUpdate(string saleId, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<SaleContent>> GetSaleContentsForUpdate(string saleId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SaleContentDetail>> GetSaleContentDetailsForUpdate(IEnumerable<int> saleContentIds,
        bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Sale>> GetSales(DateTime rangeStart, DateTime rangeEnd, int page, int viewCount, bool track = true,
        string? sortBy = null, string? searchTerm = null, string? buyerId = null,
        int? currencyId = null, CancellationToken cancellationToken = default);
}