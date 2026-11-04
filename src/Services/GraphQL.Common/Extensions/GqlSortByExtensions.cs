using GraphQL.Common.Types;
using Application.Common.Querying;

namespace GraphQL.Common.Extensions;

public static class GqlSortByExtensions
{
	public static SortExpression[] ToSortExpressions(this IEnumerable<GqlSortBy>? sorting) =>
		sorting?.Select(sort => sort.ToSortExpression()).ToArray() ?? [];
}
