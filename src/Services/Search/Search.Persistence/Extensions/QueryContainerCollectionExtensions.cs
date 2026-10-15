using OpenSearch.Client;

namespace Search.Persistence.Extensions;

internal static class QueryContainerCollectionExtensions
{
	internal static ICollection<QueryContainer> ApplyWhenNotNull<T>(
		this ICollection<QueryContainer> filters,
		T? value,
		Func<T, QueryContainer> build) where T : class
	{
		if (value is not null)
			filters.Add(build(value));
		return filters;
	}
}
