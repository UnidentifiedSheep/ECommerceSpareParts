namespace Application.Common.Querying;

public sealed record SortExpression(string Field, bool Descending = false, string? Argument = null)
{
	public static SortExpression Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return new SortExpression(string.Empty);

		value = value.Trim();
		var delimiter = QueryableSortBy.Value.Delimiter;
		var first = value.IndexOf(delimiter);
		if (first < 0)
			return new SortExpression(value.ToLowerInvariant());

		var last = value.LastIndexOf(delimiter);
		var field = value[..first].ToLowerInvariant();
		var direction = value[(last + 1)..].ToLowerInvariant();
		if (field.Length == 0)
			throw new ArgumentException("Sort field must not be empty.", nameof(value));
		if (direction is not ("asc" or "desc"))
			throw new ArgumentException($"Unknown sort direction '{direction}'.", nameof(value));

		var argument = first == last ? null : value[(first + 1)..last];
		if (argument is not null && string.IsNullOrWhiteSpace(argument))
			throw new ArgumentException("Sort argument must not be empty.", nameof(value));

		return new SortExpression(field, direction == "desc", argument);
	}

	public static implicit operator SortExpression(string value) => Parse(value);
}
