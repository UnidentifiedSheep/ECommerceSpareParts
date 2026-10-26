using GraphQL.Common.Types;

namespace GraphQL.Common.Extensions;

public static class GqlSortByExtensions
{
	public static string[] ToSortExpressions(this IEnumerable<GqlSortBy>? sorting) =>
		sorting?.Select(sort => sort.ToSortExpression()).ToArray() ?? [];
}
