using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Static;
using MediatR;

namespace Main.Application.Handlers.Uploads;

public record CompleteUploadCommand(string FileName) : ICommand;

public class CompleteUploadHandler(
    IS3StorageService s3Service
) : ICommandHandler<CompleteUploadCommand>
{
    public async Task<Unit> Handle(
        CompleteUploadCommand request,
        CancellationToken cancellationToken)
    {
        await s3Service.CompletePresignedUploadUrl(
            BucketNames.Uploads,
            request.FileName,
            cancellationToken);

        return Unit.Value;
    }
}