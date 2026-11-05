using System.Runtime.CompilerServices;
using System.Text.Json;
using Domain.Interfaces;

namespace Domain.Keys;

public static class EntityKey
{
	/// <summary>Returns key components in primary key order.</summary>
	public static object[] GetValues(object? key) => key switch
	{
		null => [],
		ITuple tuple => GetTupleValues(tuple),
		ICompositeKey composite => composite.ToArray(),
		_ => [key]
	};

	/// <summary>Formats a scalar or composite key as a JSON array.</summary>
	public static string Format(object key)
	{
		ArgumentNullException.ThrowIfNull(key);
		return FormatValues(GetValues(key));
	}

	/// <summary>Formats primary key components using the same representation as domain keys.</summary>
	public static string FormatValues(IReadOnlyList<object> values)
	{
		ArgumentNullException.ThrowIfNull(values);
		return JsonSerializer.Serialize(values);
	}

	private static object[] GetTupleValues(ITuple tuple)
	{
		var values = new object[tuple.Length];
		for (var i = 0; i < tuple.Length; i++)
			values[i] = tuple[i]!;
		return values;
	}
}
