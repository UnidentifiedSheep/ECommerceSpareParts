using System.Linq.Expressions;

namespace Application.Common.Querying;

public sealed record KeySelectorSortDefinition<TEntity>(
	Expression<Func<TEntity, object?>> KeySelector,
	bool Desc);
