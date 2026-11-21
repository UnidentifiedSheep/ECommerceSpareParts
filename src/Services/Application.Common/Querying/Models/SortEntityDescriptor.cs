using System.Linq.Expressions;

namespace Application.Common.Querying.Models;

public sealed class SortEntityDescriptor(Type entityType)
{
	private readonly Dictionary<string, SortFieldDescriptor> _fields = new(StringComparer.OrdinalIgnoreCase);

	public Type EntityType { get; } = entityType;

	public LambdaExpression? DefaultSelector { get; internal set; }

	public bool DefaultDescending { get; internal set; }

	public IReadOnlyCollection<SortFieldDescriptor> Fields => _fields.Values;

	public SortFieldDescriptor? FindField(string name)
		=> _fields.GetValueOrDefault(name);

	internal SortFieldDescriptor GetOrAddField(string name, Type keyType)
	{
		if (!_fields.TryGetValue(name, out var field))
		{
			field = new SortFieldDescriptor(name, keyType);
			_fields[name] = field;
			return field;
		}

		if (!string.Equals(field.Name, name, StringComparison.Ordinal))
			throw new InvalidOperationException(
				$"Sort field '{name}' of {EntityType.Name} differs only by case from '{field.Name}'.");
		return field;
	}
}
