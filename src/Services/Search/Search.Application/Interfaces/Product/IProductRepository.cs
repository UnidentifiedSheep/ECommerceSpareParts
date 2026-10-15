using Search.Application.Models.CatalogueSearch;
using Search.Application.Models;

namespace Search.Application.Interfaces.Product;

public interface IProductRepository : ISearchRepository<Entities.Product, int>
{
	Task ApplyUpdates(IReadOnlyCollection<ProductFieldUpdate> updates, CancellationToken cancellationToken = default);

	Task<SearchResult<Entities.Product>> Search(
		CatalogueSearchCriteria criteria,
		CancellationToken cancellationToken = default);
}
