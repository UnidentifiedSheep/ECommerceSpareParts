using NamedObject.Core.Interfaces;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentDefinition<in TRequest> : IDocumentDefinition
	where TRequest : IDocumentRequest
{
	Task GenerateAsync(
		TRequest request,
		Stream destination,
		CancellationToken token = default);

	Task IDocumentDefinition.GenerateAsync(
		IDocumentRequest data,
		Stream destination,
		CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(data);

		if (data is not TRequest request)
			throw new ArgumentException(
				$"Expected '{typeof(TRequest).FullName}', " +
				$"got '{data.GetType().FullName}'.");

		return GenerateAsync(request, destination, token);
	}
}

public interface IDocumentDefinition : INamedObject
{
	Task GenerateAsync(
		IDocumentRequest data,
		Stream destination,
		CancellationToken token = default);
}
