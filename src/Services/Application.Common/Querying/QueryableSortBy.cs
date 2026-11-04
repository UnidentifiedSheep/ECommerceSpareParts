using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;

namespace Application.Common.Querying;

public class QueryableSortBy
{
	private const string DefaultKey = "__default__";

	public static readonly QueryableSortBy Value = new();

	private readonly ConcurrentDictionary<Type, bool> _defaultDirectionMap = new();

	private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _mapDictionary = new();

	private readonly ConcurrentDictionary<(Type Entity, string Field), Func<string, LambdaExpression>> _parameterizedMappings = new();

	public char Delimiter { get; private set; } = '_';

	public void SetDelimiter(char delimiter) => Delimiter = delimiter;

	public char GetDelimiter() => Delimiter;

	public QueryableSortBy Map<TSource, TKey>(string source, Expression<Func<TSource, TKey>> keySelector)
	{
		var type = typeof(TSource);
		source = source.ToLowerInvariant();

		var primary = _mapDictionary.GetOrAdd(type, _ => new ConcurrentDictionary<string, object>());

		var objectSelector = Expression.Lambda<Func<TSource, object>>(
			Expression.Convert(keySelector.Body, typeof(object)),
			keySelector.Parameters);

		primary[source] = objectSelector;
		return this;
	}

	public QueryableSortBy MapParameterized<TEntity, TArgument, TKey>(
		string field,
		Expression<Func<TEntity, TArgument, TKey>> selector)
	{
		_parameterizedMappings[(typeof(TEntity), field.ToLowerInvariant())] = argument =>
		{
			var value = ConvertArgument<TArgument>(argument);
			var body = new ParameterSubstitution(selector.Parameters[1],
				Expression.Constant(value, typeof(TArgument))).Visit(selector.Body)!;
			return Expression.Lambda<Func<TEntity, object?>>(
				Expression.Convert(body, typeof(object)), selector.Parameters[0]);
		};
		return this;
	}

	private static TArgument ConvertArgument<TArgument>(string argument)
	{
		try
		{
			var type = typeof(TArgument);
			var value = TypeDescriptor.GetConverter(type).ConvertFrom(null, CultureInfo.InvariantCulture, argument);
			if (type.IsEnum && (value is null || !Enum.IsDefined(type, value)))
				throw new ArgumentException($"Unknown {type.Name} value '{argument}'.");
			return (TArgument)value!;
		}
		catch (Exception exception) when (exception is ArgumentException or FormatException or OverflowException or NotSupportedException)
		{
			throw new ArgumentException($"Invalid sort argument '{argument}' for {typeof(TArgument).Name}.", exception);
		}
	}

	private sealed class ParameterSubstitution(ParameterExpression parameter, Expression replacement) : ExpressionVisitor
	{
		protected override Expression VisitParameter(ParameterExpression node) =>
			node == parameter ? replacement : base.VisitParameter(node);
	}

	public QueryableSortBy MapDefault<TSource, TKey>(
		Expression<Func<TSource, TKey>> keySelector,
		bool desc = false)
	{
		var type = typeof(TSource);

		var primary = _mapDictionary.GetOrAdd(type, _ => new ConcurrentDictionary<string, object>());

		var objectSelector = Expression.Lambda<Func<TSource, object>>(
			Expression.Convert(keySelector.Body, typeof(object)),
			keySelector.Parameters);

		primary[DefaultKey] = objectSelector;
		_defaultDirectionMap[type] = desc;
		return this;
	}

	public Expression<Func<TEntity, object?>> GetMapping<TEntity>(string source, string? argument = null)
	{
		source = source.ToLowerInvariant();
		var type = typeof(TEntity);

		if (argument is not null)
		{
			if (!_parameterizedMappings.TryGetValue((type, source), out var mapping))
				throw new ArgumentException($"Sort mapping '{source}' for {type} does not accept an argument.");
			return (Expression<Func<TEntity, object?>>)mapping(argument);
		}

		if (!_mapDictionary.TryGetValue(type, out var primary))
			throw new ArgumentException($"{type} mapping not exists");

		if (primary.TryGetValue(source, out var value))
			return (Expression<Func<TEntity, object?>>)value;

		if (string.IsNullOrEmpty(source) && primary.TryGetValue(DefaultKey, out var defaultValue))
			return (Expression<Func<TEntity, object?>>)defaultValue;

		throw new ArgumentException($"Sort mapping '{source}' for {type} does not exist.");
	}

	public bool GetDefaultDesc<TEntity>() =>
		_defaultDirectionMap.TryGetValue(typeof(TEntity), out var desc) && desc;

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

}
