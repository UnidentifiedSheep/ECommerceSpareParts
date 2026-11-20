using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using Application.Common.Querying.Models;

namespace Application.Common.Querying;

public class QueryableSortBy
{
	public static readonly QueryableSortBy Value = new();

	private readonly ConcurrentDictionary<Type, SortEntityDescriptor> _entities = new();

	private readonly Lock _lock = new();

	public char Delimiter { get; private set; } = '_';

	public void SetDelimiter(char delimiter) => Delimiter = delimiter;

	public char GetDelimiter() => Delimiter;

	public QueryableSortBy Map<TSource, TKey>(string source, Expression<Func<TSource, TKey>> keySelector)
	{
		lock (_lock)
		{
			var field = GetOrAddEntity(typeof(TSource)).GetOrAddField(source, typeof(TKey));
			field.KeyType = typeof(TKey);
			field.Selector = ToObjectSelector(keySelector);
		}

		return this;
	}

	public QueryableSortBy MapParameterized<TEntity, TArgument, TKey>(
		string field,
		Expression<Func<TEntity, TArgument, TKey>> selector)
	{
		lock (_lock)
		{
			var descriptor = GetOrAddEntity(typeof(TEntity)).GetOrAddField(field, typeof(TKey));
			descriptor.ArgumentType = typeof(TArgument);
			descriptor.ParameterizedSelector = argument =>
			{
				var value = ConvertArgument<TArgument>(argument);
				var body = new ParameterSubstitution(
					selector.Parameters[1],
					Expression.Constant(value, typeof(TArgument))).Visit(selector.Body)!;
				return Expression.Lambda<Func<TEntity, object?>>(
					Expression.Convert(body, typeof(object)),
					selector.Parameters[0]);
			};
		}

		return this;
	}

	public QueryableSortBy MapDefault<TSource, TKey>(
		Expression<Func<TSource, TKey>> keySelector,
		bool desc = false)
	{
		lock (_lock)
		{
			var entity = GetOrAddEntity(typeof(TSource));
			entity.DefaultSelector = ToObjectSelector(keySelector);
			entity.DefaultDescending = desc;
		}

		return this;
	}

	public SortEntityDescriptor? FindEntity(Type entityType)
		=> _entities.GetValueOrDefault(entityType);

	public IReadOnlyCollection<SortFieldDescriptor> GetFields(Type entityType)
		=> FindEntity(entityType)?.Fields ?? [];

	public Expression<Func<TEntity, object?>> GetMapping<TEntity>(string source, string? argument = null)
	{
		var type = typeof(TEntity);
		var entity = FindEntity(type);

		if (argument is not null)
		{
			if (entity?.FindField(source)?.ParameterizedSelector is not { } parameterized)
				throw new ArgumentException(
					$"Sort mapping '{source}' for {type} does not accept an argument.");
			return (Expression<Func<TEntity, object?>>)parameterized(argument);
		}

		if (entity is null)
			throw new ArgumentException($"{type} mapping not exists");

		if (string.IsNullOrEmpty(source))
			return entity.DefaultSelector is { } defaultSelector
				? (Expression<Func<TEntity, object?>>)defaultSelector
				: throw new ArgumentException($"Default sort mapping for {type} does not exist.");

		var field = entity.FindField(source) ??
			throw new ArgumentException($"Sort mapping '{source}' for {type} does not exist.");

		return field.Selector is { } selector
			? (Expression<Func<TEntity, object?>>)selector
			: throw new ArgumentException($"Sort mapping '{source}' for {type} requires an argument.");
	}

	public bool GetDefaultDesc<TEntity>()
		=> FindEntity(typeof(TEntity))?.DefaultDescending ?? false;

	public static KeySelectorSortDefinition<TEntity> ParseToKeySelector<TEntity>(SortExpression? sort)
	{
		var field = sort?.Field.Trim() ?? string.Empty;
		var map = Value.GetMapping<TEntity>(field, sort?.Argument);
		var desc = string.IsNullOrEmpty(field) ? Value.GetDefaultDesc<TEntity>() : sort!.Descending;
		return new KeySelectorSortDefinition<TEntity>(map, desc);
	}

	public static IReadOnlyList<KeySelectorSortDefinition<TEntity>> ParseToKeySelectors<TEntity>(
		IReadOnlyCollection<SortExpression>? sortParams)
	{
		if (sortParams is null || sortParams.Count == 0)
			return [ParseToKeySelector<TEntity>(null)];
		return sortParams.Select(ParseToKeySelector<TEntity>).ToArray();
	}

	private SortEntityDescriptor GetOrAddEntity(Type entityType)
		=> _entities.GetOrAdd(entityType, type => new SortEntityDescriptor(type));

	private static Expression<Func<TSource, object?>> ToObjectSelector<TSource, TKey>(
		Expression<Func<TSource, TKey>> keySelector)
		=> Expression.Lambda<Func<TSource, object?>>(
			Expression.Convert(keySelector.Body, typeof(object)),
			keySelector.Parameters);

	private static TArgument ConvertArgument<TArgument>(string argument)
	{
		try
		{
			var type = typeof(TArgument);
			var value = TypeDescriptor.GetConverter(type)
				.ConvertFrom(
					null,
					CultureInfo.InvariantCulture,
					argument);
			if (type.IsEnum && (value is null || !Enum.IsDefined(type, value)))
				throw new ArgumentException($"Unknown {type.Name} value '{argument}'.");
			return (TArgument)value!;
		}
		catch (Exception exception) when (exception is ArgumentException or FormatException or
											OverflowException or NotSupportedException)
		{
			throw new ArgumentException(
				$"Invalid sort argument '{argument}' for {typeof(TArgument).Name}.",
				exception);
		}
	}

	private sealed class ParameterSubstitution(ParameterExpression parameter, Expression replacement)
		: ExpressionVisitor
	{
		protected override Expression VisitParameter(ParameterExpression node)
			=> node == parameter ? replacement : base.VisitParameter(node);
	}
}
