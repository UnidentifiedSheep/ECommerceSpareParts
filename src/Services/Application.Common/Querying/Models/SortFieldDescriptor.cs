using System.Linq.Expressions;

namespace Application.Common.Querying.Models;

public sealed class SortFieldDescriptor(string name, Type keyType)
{
	public string Name { get; } = name;

	public Type KeyType { get; internal set; } = keyType;

	public LambdaExpression? Selector { get; internal set; }

	public Type? ArgumentType { get; internal set; }

	public Func<string, LambdaExpression>? ParameterizedSelector { get; internal set; }

	public bool AcceptsArgument => ParameterizedSelector is not null;

	public bool RequiresArgument => Selector is null && ParameterizedSelector is not null;
}
