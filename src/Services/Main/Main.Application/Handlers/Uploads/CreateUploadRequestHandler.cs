using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Static;

namespace Main.Application.Handlers.Uploads;

public record CreateUploadRequestCommand(string FileName, string ContentType)
    : ICommand<CreateUploadRequestResult>;

public record CreateUploadRequestResult(string UploadUrl);

public class CreateUploadRequestHandler(
    IS3StorageService storageService
) : ICommandHandler<CreateUploadRequestCommand, CreateUploadRequestResult>
{
    public async Task<CreateUploadRequestResult> Handle(
        CreateUploadRequestCommand request,
        CancellationToken cancellationToken)
    {
        var uri = await storageService.CreatePresignedUploadUrl(
            BucketNames.Uploads,
            request.FileName,
            request.ContentType,
            TimeSpan.FromMinutes(15));

        return new CreateUploadRequestResult(uri);
    }
}