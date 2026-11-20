using System.Text;
using Application.Common.Querying;
using HotChocolate.Types;

namespace GraphQL.Common.Types.Sorting;

public sealed class SortFieldEnumType<TEntity> : EnumType<string>
{
	protected override void Configure(IEnumTypeDescriptor<string> descriptor)
	{
		descriptor.Name(SortTypeNames.Create(descriptor.Extend().Context, typeof(TEntity), "SortField"));
		descriptor.BindValuesExplicitly();

		foreach (var field in QueryableSortBy.Value.GetFields(typeof(TEntity)).OrderBy(field => field.Name))
		{
			var value = descriptor.Value(field.Name).Name(ToConstantCase(field.Name));
			if (field.ArgumentType is not null)
				value.Description(
					field.RequiresArgument
						? $"Requires argument of type {field.ArgumentType.Name}."
						: $"Accepts optional argument of type {field.ArgumentType.Name}.");
		}
	}

	public static string ToConstantCase(string name)
	{
		var builder = new StringBuilder(name.Length + 8);
		for (var i = 0; i < name.Length; i++)
		{
			var current = name[i];
			if (i > 0 && char.IsUpper(current) && (char.IsLower(name[i - 1]) || char.IsDigit(name[i - 1])))
				builder.Append('_');
			builder.Append(char.ToUpperInvariant(current));
		}

		return builder.ToString();
	}
}
