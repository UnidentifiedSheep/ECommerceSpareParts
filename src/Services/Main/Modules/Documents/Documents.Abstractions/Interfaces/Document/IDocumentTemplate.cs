using Enums;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentTemplate : IDisposable
{
	DocumentType Type { get; }

	void AddData<TData>(string key, TData data);

	Task RenderAsync(Stream stream, CancellationToken token = default);
}
