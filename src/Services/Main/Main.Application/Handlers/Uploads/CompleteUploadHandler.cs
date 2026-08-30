using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Models.Options.S3;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Uploads;

public record CompleteUploadCommand(string FileName) : ICommand;

public class CompleteUploadHandler(IS3StorageService s3Service, IOptions<S3BucketsOptions> bucketsOptions)
	: ICommandHandler<CompleteUploadCommand>
{
	public async Task<Unit> Handle(CompleteUploadCommand request, CancellationToken cancellationToken)
	{
		await s3Service.CompletePresignedUploadUrl(
			bucketsOptions.Value.Uploads.Name,
			request.FileName,
			cancellationToken);

		return Unit.Value;
	}
}
