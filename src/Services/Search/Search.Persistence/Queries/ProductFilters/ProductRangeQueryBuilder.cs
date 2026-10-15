using System.Globalization;
using OpenSearch.Client;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Entities;

namespace Search.Persistence.Queries.ProductFilters;

internal static class ProductRangeQueryBuilder
{
	internal static QueryContainer Build<T>(Field field, RangeCriteria<T> range, decimal scale = 1m)
		where T : struct, IComparable<T> =>
		new QueryContainerDescriptor<Product>().Range(query =>
		{
			query.Field(field);
			if (range.Gt is { } gt) query.GreaterThan(Normalize(gt, scale));
			if (range.Gte is { } gte) query.GreaterThanOrEquals(Normalize(gte, scale));
			if (range.Lt is { } lt) query.LessThan(Normalize(lt, scale));
			if (range.Lte is { } lte) query.LessThanOrEquals(Normalize(lte, scale));
			return query;
		});

	private static double Normalize<T>(T value, decimal scale) where T : struct
	{
		try
		{
			return (double)(Convert.ToDecimal(value, CultureInfo.InvariantCulture) * scale);
		}
		catch (OverflowException)
		{
			return Convert.ToDouble(value, CultureInfo.InvariantCulture) * (double)scale;
		}
	}
}
