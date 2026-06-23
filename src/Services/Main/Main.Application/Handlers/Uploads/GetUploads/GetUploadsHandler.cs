using Abstractions.Interfaces;
using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Settings;
using Main.Application.Dtos.Uploads;
using Main.Application.Static;
using Main.Entities.Setting;

namespace Main.Application.Handlers.Uploads.GetUploads;

public record GetUploadsQuery(Cursor<string?> Cursor) : IQuery<GetUploadsResult>;
public record GetUploadsResult(
    IReadOnlyList<FileDto> Files,
    string? NextContinuationToken,
    bool HasMore);

public class GetUploadsHandler(
    IS3StorageService s3StorageService,
    ISettingsService settingsService) : IQueryHandler<GetUploadsQuery, GetUploadsResult>
{
    public async Task<GetUploadsResult> Handle(GetUploadsQuery request, CancellationToken cancellationToken)
    {
        var s3Url = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .S3ServiceUrl
            .TrimEnd('/') + $"/{BucketNames.Uploads}/";
        
        var result = await s3StorageService.ListFilesAsync(
            BucketNames.Uploads,
            request.Cursor.CursorValue,
            request.Cursor.Size,
            cancellationToken);

        var files = result
            .Files
            .Select(x => new FileDto
            {
                Key = x.Key,
                LastModified = x.LastModified,
                Size = x.Size,
                FullPath = s3Url + x.Key
            })
            .ToList();
        
        return new GetUploadsResult(
            files,
            result.NextContinuationToken,
            result.HasMore);
    }
}
