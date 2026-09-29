using System.Text;
using S3.Core.Interfaces;
using S3.Core.Models;

namespace Tests.Stubs;

public sealed class S3StorageServiceStub : IS3Service
{
	private readonly Dictionary<(string Bucket, string Key), byte[]> _files = [];

	public Task<IStreamResponse> DownloadFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		if (!_files.TryGetValue((bucketName, keyName), out var content))
			throw new FileNotFoundException($"Test S3 object '{bucketName}/{keyName}' was not found.");

		return Task.FromResult<IStreamResponse>(new MemoryStreamResponse(new MemoryStream(content, false)));
	}

	public Task<string> UploadFileAsync(
		string bucketName,
		Stream stream,
		string keyName,
		string contentType) => throw new NotSupportedException();

	public Task<bool> DeleteFileAsync(string bucketName, string keyName) => throw new NotSupportedException();

	public Task<S3ObjectListDto> ListFilesAsync(
		string bucketName,
		string? continuationToken,
		int size,
		CancellationToken ct = default) => throw new NotSupportedException();

	public Task<string> CreatePresignedUploadUrl(
		string bucketName,
		string objectKey,
		string contentType,
		TimeSpan lifetime) => throw new NotSupportedException();

	public Task CompletePresignedUploadUrl(
		string bucketName,
		string objectKey,
		CancellationToken ct = default) => throw new NotSupportedException();

	public void SetFile(
		string bucketName,
		string key,
		string content) => _files[(bucketName, key)] = Encoding.UTF8.GetBytes(content);

	private sealed class MemoryStreamResponse(Stream stream) : IStreamResponse
	{
		public Stream Stream { get; } = stream;

		public void Dispose() => Stream.Dispose();
	}
}
