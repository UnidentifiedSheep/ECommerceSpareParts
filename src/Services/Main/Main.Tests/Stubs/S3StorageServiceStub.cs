using System.Collections.Concurrent;
using System.Text;
using S3.Core.Interfaces;
using S3.Core.Models;

namespace Tests.Stubs;

public sealed class S3StorageServiceStub : IS3Service
{
	private readonly ConcurrentDictionary<(string Bucket, string Key), byte[]> _files = new();
	private readonly ConcurrentDictionary<(string Bucket, string Key), byte> _deleteFailures = new();

	public Task<Response<IStreamResponse>> DownloadFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		if (!_files.TryGetValue((bucketName, keyName), out var content))
			return Task.FromResult(Response<IStreamResponse>.Failure(System.Net.HttpStatusCode.NotFound, "NoSuchKey"));

		return Task.FromResult(Response<IStreamResponse>.Success(
			new MemoryStreamResponse(new MemoryStream(content, false))));
	}

	public Task<string> UploadFileAsync(
		string bucketName,
		Stream stream,
		string keyName,
		string contentType) => throw new NotSupportedException();

	public async Task<DeleteObjectResult> DeleteFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default)
		=> (await TryDeleteFilesAsync(bucketName, [keyName], ct))[0];

	public Task<IReadOnlyList<DeleteObjectResult>> TryDeleteFilesAsync(
		string bucketName,
		IEnumerable<string> keys,
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();
		IReadOnlyList<DeleteObjectResult> results = keys.Select(key =>
		{
			if (_deleteFailures.ContainsKey((bucketName, key)))
				return DeleteObjectResult.Fail(key, "AccessDenied", "Deletion denied by test stub.");

			_files.TryRemove((bucketName, key), out _);
			return DeleteObjectResult.Success(key);
		}).ToArray();
		return Task.FromResult(results);
	}

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

	public Task<string> CreatePresignedDownloadUrl(
		string bucketName,
		string objectKey,
		TimeSpan lifetime) => throw new NotSupportedException();

	public Task CompletePresignedUploadUrl(
		string bucketName,
		string objectKey,
		CancellationToken ct = default) => throw new NotSupportedException();

	public void SetFile(
		string bucketName,
		string key,
		string content) => _files[(bucketName, key)] = Encoding.UTF8.GetBytes(content);

	public void FailDeletion(string bucketName, string key) =>
		_deleteFailures[(bucketName, key)] = 0;

	private sealed class MemoryStreamResponse(Stream stream) : IStreamResponse
	{
		public Stream Stream { get; } = stream;

		public void Dispose() => Stream.Dispose();
	}
}
