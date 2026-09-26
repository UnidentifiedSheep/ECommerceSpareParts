using Abstractions.Models;

namespace GraphQL.Common.Types;

public record GqlPatchField<T>
{
	[GraphQLName("isSet")]
	public bool IsSet { get; init; }

	[GraphQLName("value")]
	public T? Value { get; init; }

	public static implicit operator PatchField<T>(GqlPatchField<T>? field) =>
		field is { IsSet: true }
			? PatchField<T>.From(field.Value)
			: PatchField<T>.NotSet();
}
