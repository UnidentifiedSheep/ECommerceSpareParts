using OpenSearch.Client;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Entities;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Queries.ProductFilters;

public sealed class ProductCharacteristicsFilterProvider : IProductSearchFilterProvider
{
	public IEnumerable<QueryContainer> Build(ProductSearchCriteria criteria)
	{
		foreach (var characteristic in criteria.Characteristics)
		{
			var condition = new QueryContainerDescriptor<Product>()
				.Term(term => term.Field(product => product.Characteristics.First().CharacteristicId)
					.Value(characteristic.CharacteristicId));

			if (characteristic.Values is { } values)
				condition &= new QueryContainerDescriptor<Product>().Terms(terms => terms
					.Field(product => product.Characteristics.First().Value).Terms(values));

			if (characteristic.NumericRange is { } range)
				condition &= ProductRangeQueryBuilder.Build(Infer.Field<Product>(product => product.Characteristics.First().NumericValue), range);

			var nested = new QueryContainerDescriptor<Product>().Nested(query => query
				.Path(product => product.Characteristics)
				.ScoreMode(NestedScoreMode.None)
				.Query(_ => condition));

			yield return characteristic.Exists == false
				? new QueryContainerDescriptor<Product>().Bool(query => query.MustNot(nested))
				: nested;
		}
	}
}
