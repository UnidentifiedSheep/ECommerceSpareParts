namespace Application.Common.Interfaces.Links;

public interface IAppLinkBuilder
{
	Task<Uri> CreateAsync(
		string relativeTemplate,
		IReadOnlyDictionary<string, string> parameters,
		CancellationToken cancellationToken = default);
}
