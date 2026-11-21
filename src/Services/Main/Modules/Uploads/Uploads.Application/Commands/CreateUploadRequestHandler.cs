using Application.Common.Interfaces.Cqrs;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Options;

namespace Main.Modules.Uploads.Application.Commands;

public record CreateUploadRequestCommand(string FileName, string ContentType)
	: ICommand<CreateUploadRequestResult>;

public record CreateUploadRequestResult(string UploadUrl);

public class CreateUploadRequestHandler(
	IS3Service storageService,
	IOptions<S3BucketsOptions> bucketsOptions)
	: ICommandHandler<CreateUploadRequestCommand, CreateUploadRequestResult>
{
	public async Task<CreateUploadRequestResult> Handle(
		CreateUploadRequestCommand request,
		CancellationToken cancellationToken)
	{
		var uri = await storageService.CreatePresignedUploadUrl(
			bucketsOptions.Value.Uploads.Name,
			request.FileName,
			request.ContentType,
			TimeSpan.FromMinutes(15));

		return new CreateUploadRequestResult(uri);
	}
}
