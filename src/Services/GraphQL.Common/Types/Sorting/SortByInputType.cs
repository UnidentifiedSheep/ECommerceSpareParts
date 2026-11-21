using HotChocolate.Types;

namespace GraphQL.Common.Types.Sorting;

public sealed class SortByInputType<TEntity> : InputObjectType<GqlSortBy>
{
	protected override void Configure(IInputObjectTypeDescriptor<GqlSortBy> descriptor)
	{
		descriptor.Name(SortTypeNames.Create(descriptor.Extend().Context, typeof(TEntity), "SortBy"));
		descriptor.Field(sort => sort.Field).Type<NonNullType<SortFieldEnumType<TEntity>>>();
	}
}
