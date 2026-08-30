using Search.Application.Models.CatalogueSearch;

namespace Search.Application.Interfaces.Product;

public interface IProductRepository : ISearchRepository<Entities.Product, int>
{
	Task<SearchResult<Entities.Product>> Search(
		CatalogueSearchCriteria criteria,
		CancellationToken cancellationToken = default);
}
