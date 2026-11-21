using System.Reflection;
using GraphQL.Common.Types.Sorting;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace GraphQL.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SortableByAttribute<TEntity> : InputFieldDescriptorAttribute
{
	protected override void OnConfigure(
		IDescriptorContext context,
		IInputFieldDescriptor descriptor,
		MemberInfo? member)
		=> descriptor.Type<ListType<NonNullType<SortByInputType<TEntity>>>>();
}
