using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Upload;
using Main.Application.Handlers.Uploads;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class UploadMutations
{
	[GraphQLName("createRequest")]
	[RequireAllPermissions(PermissionCodes.UPLOADS_CREATE)]
	public async Task<string> CreateRequestAsync(
		GqlCreateUploadRequestInput input,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(
				new CreateUploadRequestCommand(input.FileName, input.ContentType),
				cancellationToken))
			.UploadUrl;

	[GraphQLName("complete")]
	[RequireAllPermissions(PermissionCodes.UPLOADS_CREATE)]
	public async Task<bool> CompleteAsync(
		string fileName,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new CompleteUploadCommand(fileName), cancellationToken);
		return true;
	}
}
