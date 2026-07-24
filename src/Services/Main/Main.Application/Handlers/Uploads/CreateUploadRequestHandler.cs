using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Models.Options.S3;
using Main.Application.Static;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Uploads;

public record CreateUploadRequestCommand(string FileName, string ContentType)
    : ICommand<CreateUploadRequestResult>;

public record CreateUploadRequestResult(string UploadUrl);

public class CreateUploadRequestHandler(
    IS3StorageService storageService,
    IOptions<S3BucketsOptions> bucketsOptions
) : ICommandHandler<CreateUploadRequestCommand, CreateUploadRequestResult>
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