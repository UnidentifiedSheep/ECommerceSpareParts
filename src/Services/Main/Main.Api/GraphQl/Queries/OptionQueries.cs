using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.Options.GetEmailOptions;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class OptionQueries
{
	[GraphQLName("emails")]
	[RequireAllPermissions(PermissionCodes.OPTIONS_GET)]
	public async Task<GqlEmailOptions> GetEmailsAsync(ISender sender, CancellationToken cancellationToken)
		=> new((await sender.Send(new GetEmailOptionsQuery(), cancellationToken)).EmailOptions);
}
