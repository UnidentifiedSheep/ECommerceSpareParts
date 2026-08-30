using Abstractions.Interfaces;
using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Models.Options.S3;
using Main.Application.Dtos.Uploads;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Uploads.GetUploads;

public record GetUploadsQuery(Cursor<string?> Cursor) : IQuery<GetUploadsResult>;

public record GetUploadsResult(IReadOnlyList<FileDto> Files, string? NextContinuationToken, bool HasMore);

public class GetUploadsHandler(IS3StorageService s3StorageService, IOptions<S3BucketsOptions> bucketsOptions)
	: IQueryHandler<GetUploadsQuery, GetUploadsResult>
{
	public async Task<GetUploadsResult> Handle(GetUploadsQuery request, CancellationToken cancellationToken)
	{
		var opt = bucketsOptions.Value.Uploads;
		var result = await s3StorageService.ListFilesAsync(
			opt.Name,
			request.Cursor.CursorValue,
			request.Cursor.Size,
			cancellationToken);

		var baseUrl = opt.PublicBaseUrl.EndsWith('/') ? opt.PublicBaseUrl : opt.PublicBaseUrl + "/";
		var files = result
			.Files
			.Select(x => new FileDto
			{
				Key = x.Key,
				LastModified = x.LastModified,
				Size = x.Size,
				FullPath = baseUrl + x.Key
			})
			.ToList();

		return new GetUploadsResult(
			files,
			result.NextContinuationToken,
			result.HasMore);
	}
}
