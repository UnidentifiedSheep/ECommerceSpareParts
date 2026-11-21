using Application.Common.Dtos;
using GraphQL.Common.Types.Schema;
using HotChocolate;

namespace Main.Modules.Jobs.Api.GraphQl.Types;

[GraphQLName("JobInfo")]
public sealed record GqlJobInfo(
	[property: GraphQLIgnore]
	JobInfoDto Info)
{
	[GraphQLName("systemName")]
	public string SystemName => Info.SystemName;

	[GraphQLName("name")]
	public string Name => Info.Name;

	[GraphQLName("description")]
	public string Description => Info.Description;

	[GraphQLName("initStateSchema")]
	public GqlObjectSchema InitStateSchema => new(Info.InitStateSchema);
}
