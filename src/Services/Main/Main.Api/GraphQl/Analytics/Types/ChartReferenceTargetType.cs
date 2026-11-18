using HotChocolate.Types;
using Main.Api.GraphQl.Types.Organization;
using Main.Api.GraphQl.Types.Product;
using Main.Api.GraphQl.Types.User;

namespace Main.Api.GraphQl.Analytics.Types;

public sealed class ChartReferenceTargetType : UnionType
{
	protected override void Configure(IUnionTypeDescriptor descriptor)
	{
		descriptor.Name("ChartReferenceTarget");
		descriptor.Type<ObjectType<GqlProduct>>();
		descriptor.Type<ObjectType<GqlOrganization>>();
		descriptor.Type<ObjectType<GqlUser>>();
	}
}
