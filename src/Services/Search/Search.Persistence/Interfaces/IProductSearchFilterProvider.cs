using OpenSearch.Client;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Persistence.Interfaces;

public interface IProductSearchFilterProvider
{
	IEnumerable<QueryContainer> Build(ProductSearchCriteria criteria);
}
