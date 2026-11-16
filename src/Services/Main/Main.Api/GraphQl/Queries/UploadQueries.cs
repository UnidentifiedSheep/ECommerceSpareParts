using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Upload;
using Main.Modules.Uploads.Application.Handlers.GetUploads;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class UploadQueries
{
	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.UPLOADS_CREATE)]
	public async Task<GqlFilesPage> ListAsync(
		GqlGetUploadsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetUploadsQuery(new Cursor<string?>(input.Cursor, input.Size)),
			cancellationToken);

		return new GqlFilesPage(
			result.Files.Select(file => new GqlFile(file)).ToArray(),
			result.NextContinuationToken,
			result.HasMore);
	}
}
