using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocument : IDisposable
{
	DocumentType Type { get; }

	void AddData<TData>(string key, TData data);

	Task RenderAsync(Stream stream, CancellationToken token = default);
}
