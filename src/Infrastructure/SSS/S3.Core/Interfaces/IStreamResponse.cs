namespace S3.Core.Interfaces;

public interface IStreamResponse : IResponse
{
	Stream Stream { get; }
}
