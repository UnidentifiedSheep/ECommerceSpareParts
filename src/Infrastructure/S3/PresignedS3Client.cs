using Amazon.S3;

namespace S3;

public interface IPresignedS3Client
{
    IAmazonS3 Client { get; }

    Protocol Protocol { get; }
}

public sealed class PresignedS3Client(
    IAmazonS3 client,
    Protocol protocol
) : IPresignedS3Client
{
    public IAmazonS3 Client { get; } = client;

    public Protocol Protocol { get; } = protocol;
}