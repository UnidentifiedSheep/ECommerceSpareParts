using Amazon.S3.Model;
using S3.Core.Interfaces;

namespace S3;

internal sealed class S3StreamResponse(GetObjectResponse response) : IStreamResponse
{
	private GetObjectResponse? _response = response;

	public Stream Stream => _response?.ResponseStream ??
		throw new ObjectDisposedException(nameof(S3StreamResponse));

	public void Dispose() => Interlocked.Exchange(ref _response, null)?.Dispose();
}
