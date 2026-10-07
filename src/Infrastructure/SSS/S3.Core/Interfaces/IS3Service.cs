using S3.Core.Models;

namespace S3.Core.Interfaces;

public interface IS3Service
{
	Task<string> UploadFileAsync(
		string bucketName,
		Stream stream,
		string keyName,
		string contentType);

	Task<Response<IStreamResponse>> DownloadFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default);

	Task<DeleteObjectResult> DeleteFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default);

	Task<IReadOnlyList<DeleteObjectResult>> TryDeleteFilesAsync(
		string bucketName,
		IEnumerable<string> keys,
		CancellationToken ct = default);

	Task<S3ObjectListDto> ListFilesAsync(
		string bucketName,
		string? continuationToken,
		int size,
		CancellationToken ct = default);

	Task<string> CreatePresignedUploadUrl(
		string bucketName,
		string objectKey,
		string contentType,
		TimeSpan lifetime);

	Task<string> CreatePresignedDownloadUrl(
		string bucketName,
		string objectKey,
		TimeSpan lifetime);

	Task CompletePresignedUploadUrl(
		string bucketName,
		string objectKey,
		CancellationToken ct = default);
}
